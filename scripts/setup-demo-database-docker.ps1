param(
    [string]$ServerInstance = $env:SQL_SERVER,
    [string]$SqlUser = $env:SQL_USER,
    [string]$SqlPassword = $env:SQL_PASSWORD,
    [string]$DockerSqlContainer = "educenter-demo-sqlserver",
    [switch]$SkipSchemaInit
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ServerInstance)) {
    $ServerInstance = "localhost"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$backendRoot = Join-Path $repoRoot "backend"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Không tìm thấy docker. Hãy mở Docker Desktop hoặc thêm docker vào PATH."
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Không tìm thấy dotnet. Hãy cài .NET SDK 8 và thêm dotnet vào PATH."
}

if (-not $SqlUser -or -not $SqlPassword) {
    throw "Script Docker local cần -SqlUser và -SqlPassword để kết nối SQL container."
}

function Q([string]$Value) {
    if ($null -eq $Value) { return "NULL" }
    return "N'$($Value.Replace("'", "''"))'"
}

function D([string]$Value) {
    return "'$Value'"
}

function G([string]$Prefix, [int]$Number) {
    return "$Prefix-$($Number.ToString('000000000000'))"
}

function New-DbConnectionString([string]$Database) {
    if ($SqlUser) {
        return "Server=$ServerInstance;Database=$Database;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;Encrypt=False"
    }

    return "Server=$ServerInstance;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
}

function Invoke-EduSql([string]$Database, [string]$Query) {
    $tempFile = Join-Path ([System.IO.Path]::GetTempPath()) "educenter-$Database-$([Guid]::NewGuid()).sql"
    $containerFile = "/tmp/$(Split-Path -Leaf $tempFile)"
    $queryWithOptions = "SET ANSI_NULLS ON;`nSET QUOTED_IDENTIFIER ON;`n$Query"
    [System.IO.File]::WriteAllText($tempFile, $queryWithOptions, [System.Text.UTF8Encoding]::new($true))

    try {
        & docker cp $tempFile "${DockerSqlContainer}:$containerFile"
        if ($LASTEXITCODE -ne 0) {
            throw "docker cp failed for $tempFile."
        }

        $args = @(
            "exec", $DockerSqlContainer,
            "/opt/mssql-tools18/bin/sqlcmd",
            "-S", "localhost",
            "-d", $Database,
            "-i", $containerFile,
            "-b",
            "-C",
            "-f", "65001",
            "-l", "30",
            "-U", $SqlUser,
            "-P", $SqlPassword
        )

        & docker @args
        if ($LASTEXITCODE -ne 0) {
            throw "container sqlcmd failed for database $Database with exit code $LASTEXITCODE."
        }
    } finally {
        & docker exec -u 0 $DockerSqlContainer rm -f $containerFile *> $null
        Remove-Item -LiteralPath $tempFile -Force -ErrorAction SilentlyContinue
    }
}

function Ensure-Database([string]$Database) {
    $query = "IF DB_ID(N'$Database') IS NULL CREATE DATABASE [$Database];"
    Invoke-EduSql -Database "master" -Query $query | Out-Null
}

function Start-SchemaService {
    param(
        [string]$Name,
        [string]$ProjectPath,
        [string]$ConnectionName,
        [string]$ConnectionString,
        [string]$Database,
        [string]$ReadyTable,
        [int]$Port
    )

    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$Port"
    Set-Item -Path "Env:$ConnectionName" -Value $ConnectionString

    Write-Host "Khởi tạo schema: $Name"
    $process = Start-Process -FilePath "dotnet" -ArgumentList "run --no-launch-profile --no-restore" -WorkingDirectory $ProjectPath -PassThru -WindowStyle Hidden

    try {
        $ready = $false
        for ($attempt = 1; $attempt -le 30; $attempt++) {
            Start-Sleep -Seconds 2
            if ($process.HasExited -and $process.ExitCode -ne 0) {
                throw "$Name dừng sớm với exit code $($process.ExitCode)."
            }

            try {
                $result = Invoke-EduSql -Database $Database -Query "SET NOCOUNT ON; IF OBJECT_ID(N'$ReadyTable', 'U') IS NOT NULL SELECT 1 AS Ready;"
                if (($result -join "`n") -match "\b1\b") {
                    $ready = $true
                    break
                }
            } catch {
                Start-Sleep -Seconds 1
            }
        }

        if (-not $ready) {
            throw "$Name chưa tạo xong schema sau 60 giây."
        }
    } finally {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
        Remove-Item -Path "Env:$ConnectionName" -ErrorAction SilentlyContinue
    }
}

Write-Host "SQL Server: $ServerInstance"
if ($SqlUser) {
    Write-Host "SQL Login:  $SqlUser"
} else {
    Write-Host "SQL Login:  Windows Authentication"
}

Ensure-Database "CourseDB"
Ensure-Database "StudentDB"
Ensure-Database "PaymentDB"

if (-not $SkipSchemaInit) {
    Write-Host "Khôi phục backend NuGet packages..."
    & dotnet restore (Join-Path $backendRoot "EduCenter.Backend.slnx")
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore thất bại."
    }

    Start-SchemaService `
        -Name "CourseScheduleService" `
        -ProjectPath (Join-Path $backendRoot "CourseScheduleService") `
        -ConnectionName "ConnectionStrings__CourseDB" `
        -ConnectionString (New-DbConnectionString "CourseDB") `
        -Database "CourseDB" `
        -ReadyTable "Courses" `
        -Port 5101

    Start-SchemaService `
        -Name "StudentAttendanceService" `
        -ProjectPath (Join-Path $backendRoot "StudentAttendanceService") `
        -ConnectionName "ConnectionStrings__StudentDB" `
        -ConnectionString (New-DbConnectionString "StudentDB") `
        -Database "StudentDB" `
        -ReadyTable "Students" `
        -Port 5102

    Start-SchemaService `
        -Name "PaymentReportService" `
        -ProjectPath (Join-Path $backendRoot "PaymentReportService") `
        -ConnectionName "ConnectionStrings__PaymentDB" `
        -ConnectionString (New-DbConnectionString "PaymentDB") `
        -Database "PaymentDB" `
        -ReadyTable "UserAccounts" `
        -Port 5103
}

Write-Host "Nạp dữ liệu mẫu..."
$now = "2026-06-15T08:00:00"

$teachers = @(
    @{N=1; Name="Nguyễn Minh Khoa"; Email="teacher01@educenter.vn"; Phone="0901000001"; Spec="Frontend Vue/React"; Bio="8 năm xây dựng ứng dụng web cho doanh nghiệp, hướng dẫn theo dự án thực tế."; Exp=8; Rating="4.80"},
    @{N=2; Name="Trần Thị Thu Hà"; Email="teacher02@educenter.vn"; Phone="0901000002"; Spec="ASP.NET Core"; Bio="Chuyên thiết kế REST API, Entity Framework Core và kiến trúc microservices."; Exp=9; Rating="4.90"},
    @{N=3; Name="Lê Quốc Bảo"; Email="teacher03@educenter.vn"; Phone="0901000003"; Spec="SQL Server"; Bio="Giảng viên cơ sở dữ liệu, tối ưu truy vấn và thiết kế schema cho hệ thống nghiệp vụ."; Exp=10; Rating="4.70"},
    @{N=4; Name="Phạm Anh Tuấn"; Email="teacher04@educenter.vn"; Phone="0901000004"; Spec="UI/UX Design"; Bio="Product designer phụ trách workshop wireframe, prototype và usability testing."; Exp=7; Rating="4.60"},
    @{N=5; Name="Võ Thị Mai Linh"; Email="teacher05@educenter.vn"; Phone="0901000005"; Spec="Python Data"; Bio="Data analyst giảng dạy Python, pandas và trực quan hóa dữ liệu."; Exp=6; Rating="4.70"},
    @{N=6; Name="Đặng Hoàng Nam"; Email="teacher06@educenter.vn"; Phone="0901000006"; Spec="Java Spring Boot"; Bio="Backend engineer hướng dẫn Java, Spring Boot và kiểm thử tích hợp."; Exp=8; Rating="4.80"},
    @{N=7; Name="Huỳnh Gia Hân"; Email="teacher07@educenter.vn"; Phone="0901000007"; Spec="Software Testing"; Bio="QA lead phụ trách test case, automation cơ bản và quy trình release."; Exp=6; Rating="4.50"},
    @{N=8; Name="Bùi Đức Long"; Email="teacher08@educenter.vn"; Phone="0901000008"; Spec="DevOps"; Bio="DevOps engineer hướng dẫn Docker, CI/CD và triển khai ứng dụng web."; Exp=7; Rating="4.60"},
    @{N=9; Name="Đỗ Ngọc Anh"; Email="teacher09@educenter.vn"; Phone="0901000009"; Spec="Microsoft Office"; Bio="Chuyên luyện thi MOS Word, Excel, PowerPoint cho sinh viên và nhân viên văn phòng."; Exp=5; Rating="4.40"},
    @{N=10; Name="Phan Thành Đạt"; Email="teacher10@educenter.vn"; Phone="0901000010"; Spec="Power BI"; Bio="Tư vấn báo cáo quản trị và dashboard tài chính bằng Power BI."; Exp=6; Rating="4.70"},
    @{N=11; Name="Vũ Hương Giang"; Email="teacher11@educenter.vn"; Phone="0901000011"; Spec="Business English"; Bio="Giảng viên tiếng Anh giao tiếp doanh nghiệp và phỏng vấn tuyển dụng."; Exp=9; Rating="4.80"},
    @{N=12; Name="Hoàng Nhật Minh"; Email="teacher12@educenter.vn"; Phone="0901000012"; Spec="IELTS Foundation"; Bio="Giảng viên IELTS nền tảng, tập trung phát âm, từ vựng học thuật và writing task 1."; Exp=8; Rating="4.70"},
    @{N=13; Name="Đinh Thảo Vy"; Email="teacher13@educenter.vn"; Phone="0901000013"; Spec="Digital Marketing"; Bio="Marketing specialist hướng dẫn content, ads căn bản và đo lường chiến dịch."; Exp=5; Rating="4.50"},
    @{N=14; Name="Cao Mạnh Hùng"; Email="teacher14@educenter.vn"; Phone="0901000014"; Spec="Presentation Skills"; Bio="Trainer kỹ năng thuyết trình, kể chuyện dữ liệu và phản biện nhóm."; Exp=7; Rating="4.60"},
    @{N=15; Name="Nguyễn Thị Lan Anh"; Email="teacher15@educenter.vn"; Phone="0901000015"; Spec="C# Fundamentals"; Bio="Lập trình viên .NET giảng dạy C#, OOP và clean code cho người mới."; Exp=6; Rating="4.70"},
    @{N=16; Name="Trương Quang Huy"; Email="teacher16@educenter.vn"; Phone="0901000016"; Spec="React Advanced"; Bio="Frontend lead hướng dẫn state management, testing và tối ưu hiệu năng React."; Exp=8; Rating="4.80"},
    @{N=17; Name="Lâm Khánh Ngọc"; Email="teacher17@educenter.vn"; Phone="0901000017"; Spec="Data Visualization"; Bio="Chuyên xây dựng dashboard vận hành bằng Excel, Power BI và SQL."; Exp=6; Rating="4.60"},
    @{N=18; Name="Mai Tiến Dũng"; Email="teacher18@educenter.vn"; Phone="0901000018"; Spec="Node.js Backend"; Bio="Backend engineer giảng dạy Express, JWT, REST API và tích hợp thanh toán."; Exp=7; Rating="4.70"},
    @{N=19; Name="Tạ Minh Châu"; Email="teacher19@educenter.vn"; Phone="0901000019"; Spec="Product Design"; Bio="Product designer hướng dẫn design system và handoff cho đội frontend."; Exp=5; Rating="4.50"},
    @{N=20; Name="Hà Phương Uyên"; Email="teacher20@educenter.vn"; Phone="0901000020"; Spec="Excel Accounting"; Bio="Kế toán trưởng hướng dẫn Excel ứng dụng cho kế toán và phân tích chi phí."; Exp=10; Rating="4.80"}
)

$courses = @(
    @{N=1; Code="WEB101"; Name="Lập trình Web cơ bản"; Slug="lap-trinh-web-co-ban"; Category="Programming"; Level="Beginner"; Fee=2800000; Sessions=16; Duration="8 tuần"; Best=1; Popular=1; Views=420; Enrolled=34; Rating="4.60"},
    @{N=2; Code="VUE201"; Name="Vue.js thực chiến"; Slug="vuejs-thuc-chien"; Category="Programming"; Level="Intermediate"; Fee=3600000; Sessions=18; Duration="9 tuần"; Best=1; Popular=1; Views=510; Enrolled=41; Rating="4.70"},
    @{N=3; Code="SQL201"; Name="SQL Server từ cơ bản đến nâng cao"; Slug="sql-server-tu-co-ban-den-nang-cao"; Category="Database"; Level="Intermediate"; Fee=3400000; Sessions=18; Duration="9 tuần"; Best=0; Popular=1; Views=390; Enrolled=29; Rating="4.50"},
    @{N=4; Code="NET301"; Name="ASP.NET Core Web API"; Slug="aspnet-core-web-api"; Category="Programming"; Level="Intermediate"; Fee=4200000; Sessions=20; Duration="10 tuần"; Best=1; Popular=1; Views=560; Enrolled=46; Rating="4.80"},
    @{N=5; Code="FS401"; Name="Fullstack .NET & Vue"; Slug="fullstack-dotnet-vue"; Category="Programming"; Level="Advanced"; Fee=6200000; Sessions=28; Duration="14 tuần"; Best=1; Popular=1; Views=740; Enrolled=52; Rating="4.90"},
    @{N=6; Code="NODE201"; Name="Node.js Backend"; Slug="nodejs-backend"; Category="Programming"; Level="Intermediate"; Fee=3900000; Sessions=18; Duration="9 tuần"; Best=0; Popular=1; Views=430; Enrolled=31; Rating="4.50"},
    @{N=7; Code="UX101"; Name="Thiết kế UI/UX cho sản phẩm số"; Slug="thiet-ke-ui-ux-cho-san-pham-so"; Category="Design"; Level="Beginner"; Fee=3200000; Sessions=14; Duration="7 tuần"; Best=0; Popular=1; Views=370; Enrolled=27; Rating="4.40"},
    @{N=8; Code="MOS101"; Name="Tin học văn phòng MOS"; Slug="tin-hoc-van-phong-mos"; Category="Office"; Level="Beginner"; Fee=2400000; Sessions=12; Duration="6 tuần"; Best=0; Popular=0; Views=260; Enrolled=22; Rating="4.30"},
    @{N=9; Code="REACT301"; Name="ReactJS chuyên sâu"; Slug="reactjs-chuyen-sau"; Category="Programming"; Level="Advanced"; Fee=4500000; Sessions=20; Duration="10 tuần"; Best=1; Popular=1; Views=580; Enrolled=38; Rating="4.70"},
    @{N=10; Code="PYDATA201"; Name="Python phân tích dữ liệu"; Slug="python-phan-tich-du-lieu"; Category="Data"; Level="Intermediate"; Fee=4100000; Sessions=18; Duration="9 tuần"; Best=0; Popular=1; Views=490; Enrolled=33; Rating="4.60"},
    @{N=11; Code="JAVA301"; Name="Java Spring Boot"; Slug="java-spring-boot"; Category="Programming"; Level="Intermediate"; Fee=4300000; Sessions=20; Duration="10 tuần"; Best=0; Popular=1; Views=450; Enrolled=30; Rating="4.50"},
    @{N=12; Code="QA101"; Name="Kiểm thử phần mềm"; Slug="kiem-thu-phan-mem"; Category="Testing"; Level="Beginner"; Fee=3100000; Sessions=14; Duration="7 tuần"; Best=0; Popular=0; Views=300; Enrolled=24; Rating="4.40"},
    @{N=13; Code="DEVOPS101"; Name="DevOps căn bản với Docker"; Slug="devops-can-ban-voi-docker"; Category="DevOps"; Level="Beginner"; Fee=3800000; Sessions=16; Duration="8 tuần"; Best=0; Popular=1; Views=410; Enrolled=28; Rating="4.50"},
    @{N=14; Code="EXACC101"; Name="Excel cho kế toán"; Slug="excel-cho-ke-toan"; Category="Office"; Level="Beginner"; Fee=2600000; Sessions=12; Duration="6 tuần"; Best=0; Popular=0; Views=290; Enrolled=26; Rating="4.40"},
    @{N=15; Code="PBI201"; Name="Power BI thực chiến"; Slug="power-bi-thuc-chien"; Category="Data"; Level="Intermediate"; Fee=3700000; Sessions=16; Duration="8 tuần"; Best=0; Popular=1; Views=440; Enrolled=32; Rating="4.60"},
    @{N=16; Code="ENG101"; Name="Tiếng Anh giao tiếp doanh nghiệp"; Slug="tieng-anh-giao-tiep-doanh-nghiep"; Category="Language"; Level="Beginner"; Fee=3000000; Sessions=20; Duration="10 tuần"; Best=0; Popular=1; Views=520; Enrolled=45; Rating="4.70"},
    @{N=17; Code="IELTS101"; Name="IELTS Foundation"; Slug="ielts-foundation"; Category="Language"; Level="Beginner"; Fee=4800000; Sessions=24; Duration="12 tuần"; Best=1; Popular=1; Views=610; Enrolled=39; Rating="4.80"},
    @{N=18; Code="MKT101"; Name="Digital Marketing cơ bản"; Slug="digital-marketing-co-ban"; Category="Marketing"; Level="Beginner"; Fee=3300000; Sessions=14; Duration="7 tuần"; Best=0; Popular=0; Views=360; Enrolled=25; Rating="4.40"},
    @{N=19; Code="SOFT101"; Name="Kỹ năng thuyết trình"; Slug="ky-nang-thuyet-trinh"; Category="Soft Skills"; Level="Beginner"; Fee=2200000; Sessions=10; Duration="5 tuần"; Best=0; Popular=0; Views=250; Enrolled=21; Rating="4.30"},
    @{N=20; Code="CS101"; Name="Lập trình C# nền tảng"; Slug="lap-trinh-csharp-nen-tang"; Category="Programming"; Level="Beginner"; Fee=3500000; Sessions=16; Duration="8 tuần"; Best=0; Popular=1; Views=400; Enrolled=30; Rating="4.50"}
)

$studentNames = @(
    "Nguyễn Hoàng Phúc","Trần Minh Anh","Lê Gia Bảo","Phạm Khánh Linh","Võ Nhật Nam",
    "Đặng Phương Thảo","Huỳnh Đức Anh","Bùi Ngọc Hân","Đỗ Minh Quân","Phan Hà My",
    "Vũ Quang Huy","Hoàng Bảo Ngọc","Đinh Tuấn Kiệt","Cao Thùy Dương","Nguyễn Hải Đăng",
    "Trương Mai Chi","Lâm Quốc Việt","Mai Thanh Trúc","Tạ Anh Khoa","Hà Gia Linh",
    "Nguyễn Đức Minh","Trần Thảo Nguyên","Lê Minh Khôi","Phạm Yến Nhi","Võ Gia Huy",
    "Đặng Bảo Trân","Huỳnh Minh Khang","Bùi Phương Anh","Đỗ Gia Hưng","Phan Ngọc Mai",
    "Vũ Thành Long","Hoàng Kim Chi","Đinh Bảo Nam","Cao Ngọc Diệp","Nguyễn Anh Thư",
    "Trương Minh Đức","Lâm Hà Vy","Mai Quốc Hưng","Tạ Phương Nhi","Hà Minh Tâm"
)

$classRooms = @("A101","A203","B102","B204","C301","C302","Lab 1","Lab 2","Online Zoom 01","Online Zoom 02")

$courseLines = New-Object System.Collections.Generic.List[string]
$courseLines.Add("SET XACT_ABORT ON; BEGIN TRAN;")

foreach ($teacher in $teachers) {
    $id = G "11111111-1111-1111-1111" $teacher.N
    $courseLines.Add(@"
IF EXISTS (SELECT 1 FROM Teachers WHERE Id = '$id')
    UPDATE Teachers SET FullName=$(Q $teacher.Name), Email=$(Q $teacher.Email), Phone=$(Q $teacher.Phone), Specialization=$(Q $teacher.Spec), Bio=$(Q $teacher.Bio), ExperienceYears=$($teacher.Exp), Rating=$($teacher.Rating), Status=0, UpdatedAt=$(D $now) WHERE Id='$id';
ELSE
    INSERT INTO Teachers (Id, FullName, Email, Phone, AvatarUrl, Specialization, Bio, ExperienceYears, Rating, Status, CreatedAt, UpdatedAt)
    VALUES ('$id', $(Q $teacher.Name), $(Q $teacher.Email), $(Q $teacher.Phone), NULL, $(Q $teacher.Spec), $(Q $teacher.Bio), $($teacher.Exp), $($teacher.Rating), 0, $(D $now), $(D $now));
"@)
}

foreach ($course in $courses) {
    $id = G "22222222-2222-2222-2222" $course.N
    $desc = "Khóa học $($course.Name) tập trung vào thực hành, bài tập theo tình huống trung tâm đào tạo và sản phẩm cuối khóa."
    $short = "Lộ trình $($course.Level) cho $($course.Name)."
    $courseLines.Add(@"
IF EXISTS (SELECT 1 FROM Courses WHERE Id = '$id')
    UPDATE Courses SET Code=$(Q $course.Code), Name=$(Q $course.Name), Slug=$(Q $course.Slug), ShortDescription=$(Q $short), Description=$(Q $desc), Category=$(Q $course.Category), Level=$(Q $course.Level), TuitionFee=$($course.Fee), TotalSessions=$($course.Sessions), DurationText=$(Q $course.Duration), Status=1, IsBestSeller=$($course.Best), IsPopularThisWeek=$($course.Popular), ViewCount=$($course.Views), EnrolledCount=$($course.Enrolled), Rating=$($course.Rating), UpdatedAt=$(D $now) WHERE Id='$id';
ELSE
    INSERT INTO Courses (Id, Code, Name, Slug, ShortDescription, Description, Category, Level, TuitionFee, TotalSessions, DurationText, ThumbnailUrl, Status, IsBestSeller, IsPopularThisWeek, ViewCount, EnrolledCount, Rating, CreatedAt, UpdatedAt)
    VALUES ('$id', $(Q $course.Code), $(Q $course.Name), $(Q $course.Slug), $(Q $short), $(Q $desc), $(Q $course.Category), $(Q $course.Level), $($course.Fee), $($course.Sessions), $(Q $course.Duration), NULL, 1, $($course.Best), $($course.Popular), $($course.Views), $($course.Enrolled), $($course.Rating), $(D $now), $(D $now));
"@)
}

for ($i = 1; $i -le 20; $i++) {
    $course = $courses[$i - 1]
    $teacher = $teachers[($i - 1) % $teachers.Count]
    $classId = G "33333333-3333-3333-3333" $i
    $courseId = G "22222222-2222-2222-2222" $i
    $teacherId = G "11111111-1111-1111-1111" $teacher.N
    $code = "$($course.Code)-0626-$([char](64 + (($i - 1) % 3) + 1))"
    $className = "$($course.Name) - lớp tối $([char](64 + (($i - 1) % 3) + 1))"
    $room = $classRooms[($i - 1) % $classRooms.Count]
    $max = 24 + (($i - 1) % 4) * 3
    $current = [Math]::Min($max - 2, 10 + (($i * 3) % 18))
    $start = (Get-Date "2026-06-22").AddDays(($i - 1) * 3).ToString("yyyy-MM-ddTHH:mm:ss")
    $end = (Get-Date $start).AddDays(56 + (($i - 1) % 5) * 7).ToString("yyyy-MM-ddTHH:mm:ss")
    $mode = (($i - 1) % 3)
    $status = if ($i -le 8) { 2 } else { 0 }
    $courseLines.Add(@"
IF EXISTS (SELECT 1 FROM Classes WHERE Id = '$classId')
    UPDATE Classes SET CourseId='$courseId', CourseNameSnapshot=$(Q $course.Name), ClassCode=$(Q $code), ClassName=$(Q $className), TeacherId='$teacherId', TeacherNameSnapshot=$(Q $teacher.Name), Room=$(Q $room), MaxStudents=$max, CurrentStudents=$current, StartDate=$(D $start), EndDate=$(D $end), LearningMode=$mode, Status=$status, UpdatedAt=$(D $now) WHERE Id='$classId';
ELSE
    INSERT INTO Classes (Id, CourseId, CourseNameSnapshot, ClassCode, ClassName, TeacherId, TeacherNameSnapshot, Room, MaxStudents, CurrentStudents, StartDate, EndDate, LearningMode, Status, CreatedAt, UpdatedAt)
    VALUES ('$classId', '$courseId', $(Q $course.Name), $(Q $code), $(Q $className), '$teacherId', $(Q $teacher.Name), $(Q $room), $max, $current, $(D $start), $(D $end), $mode, $status, $(D $now), $(D $now));
"@)
}

for ($i = 1; $i -le 20; $i++) {
    $classId = G "33333333-3333-3333-3333" $i
    $scheduleId = G "44444444-4444-4444-4444" $i
    $day = 1 + (($i - 1) % 6)
    $shift = ($i - 1) % 3
    $startTimes = @("08:00:00","13:30:00","18:00:00")
    $endTimes = @("10:00:00","15:30:00","20:00:00")
    $startTime = $startTimes[$shift]
    $endTime = $endTimes[$shift]
    $room = $classRooms[($i - 1) % $classRooms.Count]
    $scheduleTopics = @("Khai giảng và định hướng học tập","Thực hành nền tảng","Workshop theo nhóm","Ôn tập và chữa bài")
    $topic = $scheduleTopics[(($i - 1) % 4)]
    $courseLines.Add(@"
IF EXISTS (SELECT 1 FROM Schedules WHERE Id = '$scheduleId')
    UPDATE Schedules SET ClassId='$classId', ClassNameSnapshot=(SELECT ClassName FROM Classes WHERE Id='$classId'), DayOfWeek=$day, StudyShift=$shift, StartTime='$startTime', EndTime='$endTime', Room=$(Q $room), Topic=$(Q $topic), SessionNumber=1 + (($i - 1) % 4), Status=0, UpdatedAt=$(D $now) WHERE Id='$scheduleId';
ELSE
    INSERT INTO Schedules (Id, ClassId, ClassNameSnapshot, DayOfWeek, StudyShift, StartTime, EndTime, Room, Topic, SessionNumber, Status, CreatedAt, UpdatedAt)
    VALUES ('$scheduleId', '$classId', (SELECT ClassName FROM Classes WHERE Id='$classId'), $day, $shift, '$startTime', '$endTime', $(Q $room), $(Q $topic), 1 + (($i - 1) % 4), 0, $(D $now), $(D $now));
"@)
}

$courseLines.Add("COMMIT;")
Invoke-EduSql -Database "CourseDB" -Query ($courseLines -join [Environment]::NewLine)

$studentLines = New-Object System.Collections.Generic.List[string]
$studentLines.Add("SET XACT_ABORT ON; BEGIN TRAN;")

for ($i = 1; $i -le 40; $i++) {
    $studentId = G "aaaaaaaa-aaaa-aaaa-aaaa" $i
    $name = $studentNames[$i - 1]
    $email = "student$($i.ToString('00'))@educenter.vn"
    $phone = "091$($i.ToString('0000000'))"
    $dob = (Get-Date "2001-01-05").AddDays($i * 37).ToString("yyyy-MM-ddTHH:mm:ss")
    $gender = if ($i % 2 -eq 0) { 2 } else { 1 }
    $address = @("Hà Nội","TP. Hồ Chí Minh","Đà Nẵng","Hải Phòng","Cần Thơ","Bình Dương")[(($i - 1) % 6)]
    $code = "STU$($i.ToString('000'))"
    $studentLines.Add(@"
IF EXISTS (SELECT 1 FROM Students WHERE Id = '$studentId')
    UPDATE Students SET StudentCode=$(Q $code), FullName=$(Q $name), Email=$(Q $email), Phone=$(Q $phone), DateOfBirth=$(D $dob), Gender=$gender, Address=$(Q $address), Status=1, UpdatedAt=$(D $now) WHERE Id='$studentId';
ELSE
    INSERT INTO Students (Id, StudentCode, FullName, Email, Phone, DateOfBirth, Gender, Address, AvatarUrl, Status, CreatedAt, UpdatedAt)
    VALUES ('$studentId', $(Q $code), $(Q $name), $(Q $email), $(Q $phone), $(D $dob), $gender, $(Q $address), NULL, 1, $(D $now), $(D $now));
"@)
}

for ($i = 21; $i -le 40; $i++) {
    $studentId = G "aaaaaaaa-aaaa-aaaa-aaaa" $i
    $enrollmentId = G "cccccccc-cccc-cccc-cccc" $i
    $classIndex = (($i - 21) % 20) + 1
    $courseId = G "22222222-2222-2222-2222" $classIndex
    $classId = G "33333333-3333-3333-3333" $classIndex
    $enrolledAt = (Get-Date "2026-06-15").AddDays($i - 21).ToString("yyyy-MM-ddTHH:mm:ss")
    $status = if ($i % 5 -eq 0) { 2 } elseif ($i % 4 -eq 0) { 1 } else { 3 }
    $studentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM Enrollments WHERE Id = '$enrollmentId')
    INSERT INTO Enrollments (Id, StudentId, StudentNameSnapshot, CourseId, CourseNameSnapshot, ClassId, ClassNameSnapshot, EnrolledAt, Status, Note, CreatedAt, UpdatedAt)
    VALUES ('$enrollmentId', '$studentId', (SELECT FullName FROM Students WHERE Id='$studentId'), '$courseId', (SELECT Name FROM CourseDB.dbo.Courses WHERE Id='$courseId'), '$classId', (SELECT ClassName FROM CourseDB.dbo.Classes WHERE Id='$classId'), $(D $enrolledAt), $status, N'Ghi danh từ bộ dữ liệu demo thực tế', $(D $now), $(D $now));
"@)
}

for ($i = 10; $i -le 29; $i++) {
    $sessionId = G "bbbbbbbb-bbbb-bbbb-bbbb" $i
    $classIndex = (($i - 10) % 20) + 1
    $classId = G "33333333-3333-3333-3333" $classIndex
    $scheduleId = G "44444444-4444-4444-4444" $classIndex
    $teacherIndex = (($classIndex - 1) % 20) + 1
    $teacherId = G "11111111-1111-1111-1111" $teacherIndex
    $date = (Get-Date "2026-07-01").AddDays($i - 10).ToString("yyyy-MM-ddTHH:mm:ss")
    $topic = "Buổi $($i - 9): thực hành và kiểm tra tiến độ"
    $studentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM AttendanceSessions WHERE Id = '$sessionId')
    INSERT INTO AttendanceSessions (Id, ClassId, ClassNameSnapshot, ScheduleId, SessionNumber, AttendanceDate, Topic, CreatedByTeacherId, CreatedByTeacherName, Status, CreatedAt, UpdatedAt)
    VALUES ('$sessionId', '$classId', (SELECT ClassName FROM CourseDB.dbo.Classes WHERE Id='$classId'), '$scheduleId', (($i - 10) % 6) + 1, $(D $date), $(Q $topic), '$teacherId', (SELECT FullName FROM CourseDB.dbo.Teachers WHERE Id='$teacherId'), 1, $(D $now), $(D $now));
"@)
}

for ($i = 21; $i -le 40; $i++) {
    $recordId = G "edededed-eded-eded-eded" $i
    $sessionId = G "bbbbbbbb-bbbb-bbbb-bbbb" (10 + (($i - 21) % 20))
    $studentId = G "aaaaaaaa-aaaa-aaaa-aaaa" $i
    $status = if ($i % 9 -eq 0) { 2 } elseif ($i % 7 -eq 0) { 3 } else { 1 }
    $studentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM AttendanceRecords WHERE Id = '$recordId')
    INSERT INTO AttendanceRecords (Id, AttendanceSessionId, StudentId, StudentNameSnapshot, Status, Note, MarkedAt)
    VALUES ('$recordId', '$sessionId', '$studentId', (SELECT FullName FROM Students WHERE Id='$studentId'), $status, NULL, $(D $now));
"@)
}

for ($i = 21; $i -le 40; $i++) {
    $resultId = G "eeeeeeee-eeee-eeee-eeee" $i
    $studentId = G "aaaaaaaa-aaaa-aaaa-aaaa" $i
    $classIndex = (($i - 21) % 20) + 1
    $courseId = G "22222222-2222-2222-2222" $classIndex
    $classId = G "33333333-3333-3333-3333" $classIndex
    $teacherIndex = (($classIndex - 1) % 20) + 1
    $teacherId = G "11111111-1111-1111-1111" $teacherIndex
    $mid = 6 + (($i - 21) % 4)
    $final = 6.5 + (($i - 21) % 5) * 0.5
    $avg = [Math]::Round($mid * 0.4 + $final * 0.6, 2).ToString("0.00", [Globalization.CultureInfo]::InvariantCulture)
    $attendance = 75 + (($i - 21) % 5) * 5
    $status = if ([double]$avg -ge 5 -and $attendance -ge 70) { 2 } else { 3 }
    $studentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM StudentResults WHERE Id = '$resultId')
    INSERT INTO StudentResults (Id, StudentId, StudentNameSnapshot, CourseId, CourseNameSnapshot, ClassId, ClassNameSnapshot, MidtermScore, FinalScore, AverageScore, AttendancePercent, ResultStatus, Feedback, EvaluatedByTeacherId, EvaluatedByTeacherName, EvaluatedAt, CreatedAt, UpdatedAt)
    VALUES ('$resultId', '$studentId', (SELECT FullName FROM Students WHERE Id='$studentId'), '$courseId', (SELECT Name FROM CourseDB.dbo.Courses WHERE Id='$courseId'), '$classId', (SELECT ClassName FROM CourseDB.dbo.Classes WHERE Id='$classId'), $mid, $final, $avg, $attendance, $status, N'Hoàn thành bài tập đúng tiến độ, cần tiếp tục luyện thực hành.', '$teacherId', (SELECT FullName FROM CourseDB.dbo.Teachers WHERE Id='$teacherId'), $(D $now), $(D $now), $(D $now));
"@)
}

$studentLines.Add(@"
UPDATE e SET StudentNameSnapshot = s.FullName FROM Enrollments e JOIN Students s ON e.StudentId = s.Id;
UPDATE e SET CourseNameSnapshot = c.Name FROM Enrollments e JOIN CourseDB.dbo.Courses c ON e.CourseId = c.Id;
UPDATE e SET ClassNameSnapshot = c.ClassName FROM Enrollments e JOIN CourseDB.dbo.Classes c ON e.ClassId = c.Id;
UPDATE a SET StudentNameSnapshot = s.FullName FROM AttendanceRecords a JOIN Students s ON a.StudentId = s.Id;
UPDATE ses SET ClassNameSnapshot = c.ClassName FROM AttendanceSessions ses JOIN CourseDB.dbo.Classes c ON ses.ClassId = c.Id;
UPDATE ses SET CreatedByTeacherName = t.FullName FROM AttendanceSessions ses JOIN CourseDB.dbo.Teachers t ON ses.CreatedByTeacherId = t.Id;
UPDATE r SET StudentNameSnapshot = s.FullName FROM StudentResults r JOIN Students s ON r.StudentId = s.Id;
UPDATE r SET CourseNameSnapshot = c.Name FROM StudentResults r JOIN CourseDB.dbo.Courses c ON r.CourseId = c.Id;
UPDATE r SET ClassNameSnapshot = c.ClassName FROM StudentResults r JOIN CourseDB.dbo.Classes c ON r.ClassId = c.Id;
UPDATE r SET EvaluatedByTeacherName = t.FullName FROM StudentResults r JOIN CourseDB.dbo.Teachers t ON r.EvaluatedByTeacherId = t.Id;
"@)

$studentLines.Add("COMMIT;")
Invoke-EduSql -Database "StudentDB" -Query ($studentLines -join [Environment]::NewLine)

$paymentLines = New-Object System.Collections.Generic.List[string]
$paymentLines.Add(@"
SET XACT_ABORT ON;
BEGIN TRAN;
DECLARE @TeacherHash nvarchar(500) = (SELECT TOP 1 PasswordHash FROM UserAccounts WHERE Username = 'teacher01');
DECLARE @StudentHash nvarchar(500) = (SELECT TOP 1 PasswordHash FROM UserAccounts WHERE Username = 'student01');
IF @TeacherHash IS NULL SET @TeacherHash = (SELECT TOP 1 PasswordHash FROM UserAccounts);
IF @StudentHash IS NULL SET @StudentHash = (SELECT TOP 1 PasswordHash FROM UserAccounts);
"@)

$paymentLines.Add("UPDATE UserAccounts SET FullName=N'Quản trị hệ thống', Email=N'admin@educenter.vn', Phone=N'0909000001', Role=1, ReferenceId=NULL, Status=1, UpdatedAt=$(D $now) WHERE Username='admin';")

for ($i = 1; $i -le 20; $i++) {
    $teacherId = G "11111111-1111-1111-1111" $i
    $teacher = $teachers[$i - 1]
    $username = "teacher$($i.ToString('00'))"
    $accountId = G "abababab-abab-abab-abab" $i
    $paymentLines.Add(@"
IF EXISTS (SELECT 1 FROM UserAccounts WHERE Username = '$username')
    UPDATE UserAccounts SET FullName=$(Q $teacher.Name), Email=$(Q $teacher.Email), Phone=$(Q $teacher.Phone), Role=2, ReferenceId='$teacherId', Status=1, UpdatedAt=$(D $now) WHERE Username='$username';
ELSE
    INSERT INTO UserAccounts (Id, Username, PasswordHash, FullName, Email, Phone, Role, ReferenceId, Status, CreatedAt, UpdatedAt)
    VALUES ('$accountId', '$username', @TeacherHash, $(Q $teacher.Name), $(Q $teacher.Email), $(Q $teacher.Phone), 2, '$teacherId', 1, $(D $now), $(D $now));
"@)
}

for ($i = 1; $i -le 40; $i++) {
    $studentId = G "aaaaaaaa-aaaa-aaaa-aaaa" $i
    $name = $studentNames[$i - 1]
    $username = "student$($i.ToString('00'))"
    $email = "student$($i.ToString('00'))@educenter.vn"
    $phone = "091$($i.ToString('0000000'))"
    $accountId = G "acacacac-acac-acac-acac" $i
    $paymentLines.Add(@"
IF EXISTS (SELECT 1 FROM UserAccounts WHERE Username = '$username')
    UPDATE UserAccounts SET FullName=$(Q $name), Email=$(Q $email), Phone=$(Q $phone), Role=3, ReferenceId='$studentId', Status=1, UpdatedAt=$(D $now) WHERE Username='$username';
ELSE
    INSERT INTO UserAccounts (Id, Username, PasswordHash, FullName, Email, Phone, Role, ReferenceId, Status, CreatedAt, UpdatedAt)
    VALUES ('$accountId', '$username', @StudentHash, $(Q $name), $(Q $email), $(Q $phone), 3, '$studentId', 1, $(D $now), $(D $now));
"@)
}

for ($i = 21; $i -le 40; $i++) {
    $invoiceId = G "99999999-9999-9999-9999" $i
    $enrollmentId = G "cccccccc-cccc-cccc-cccc" $i
    $studentId = G "aaaaaaaa-aaaa-aaaa-aaaa" $i
    $classIndex = (($i - 21) % 20) + 1
    $courseId = G "22222222-2222-2222-2222" $classIndex
    $classId = G "33333333-3333-3333-3333" $classIndex
    $invoiceCode = "INV2026$($i.ToString('000'))"
    $total = $courses[$classIndex - 1].Fee
    $paid = if ($i % 4 -eq 0) { $total } elseif ($i % 3 -eq 0) { [Math]::Round($total * 0.5, 0) } else { 0 }
    $debt = $total - $paid
    $due = (Get-Date "2026-07-15").AddDays(($i - 21) % 10).ToString("yyyy-MM-ddTHH:mm:ss")
    $status = if ($debt -eq 0) { 3 } elseif ($paid -gt 0) { 2 } else { 1 }
    $paymentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM TuitionInvoices WHERE Id = '$invoiceId')
    INSERT INTO TuitionInvoices (Id, EnrollmentId, InvoiceCode, StudentId, StudentNameSnapshot, CourseId, CourseNameSnapshot, ClassId, ClassNameSnapshot, TotalAmount, PaidAmount, DebtAmount, DueDate, PartialPaymentDueDate, Status, CreatedAt, UpdatedAt)
    VALUES ('$invoiceId', '$enrollmentId', $(Q $invoiceCode), '$studentId', (SELECT FullName FROM StudentDB.dbo.Students WHERE Id='$studentId'), '$courseId', (SELECT Name FROM CourseDB.dbo.Courses WHERE Id='$courseId'), '$classId', (SELECT ClassName FROM CourseDB.dbo.Classes WHERE Id='$classId'), $total, $paid, $debt, $(D $due), NULL, $status, $(D $now), $(D $now));
"@)
}

for ($i = 21; $i -le 40; $i++) {
    $paymentId = G "88888888-8888-8888-8888" $i
    $invoiceId = G "99999999-9999-9999-9999" $i
    $method = 2 + (($i - 21) % 3)
    $date = (Get-Date "2026-07-16").AddDays($i - 21).ToString("yyyy-MM-ddTHH:mm:ss")
    $status = if ($i % 6 -eq 0) { 2 } else { 1 }
    $paymentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM PaymentTransactions WHERE Id = '$paymentId')
    INSERT INTO PaymentTransactions (Id, InvoiceId, Amount, Method, PaymentDate, Status, Note, CreatedBy, CreatedAt)
    SELECT '$paymentId', '$invoiceId',
           CASE WHEN DebtAmount = 0 THEN PaidAmount ELSE CASE WHEN PaidAmount > 0 THEN PaidAmount ELSE TotalAmount * 0.5 END END,
           $method, $(D $date), $status, N'Thanh toán học phí từ dữ liệu demo thực tế', N'admin', $(D $date)
    FROM TuitionInvoices WHERE Id = '$invoiceId';
"@)
}

for ($i = 1; $i -le 20; $i++) {
    $reportId = G "bcbcbcbc-bcbc-bcbc-bcbc" $i
    $date = (Get-Date "2026-06-01").AddDays($i - 1).ToString("yyyy-MM-ddTHH:mm:ss")
    $revenue = 18000000 + $i * 1350000
    $debt = 9500000 + (($i % 5) * 1200000)
    $paidCount = 8 + ($i % 7)
    $unpaidCount = 4 + ($i % 5)
    $paymentLines.Add(@"
IF NOT EXISTS (SELECT 1 FROM RevenueReportSnapshots WHERE Id = '$reportId')
    INSERT INTO RevenueReportSnapshots (Id, ReportDate, TotalRevenue, TotalDebt, TotalPaidInvoices, TotalUnpaidInvoices, CreatedAt)
    VALUES ('$reportId', $(D $date), $revenue, $debt, $paidCount, $unpaidCount, $(D $now));
"@)
}

$paymentLines.Add(@"
UPDATE i SET StudentNameSnapshot = s.FullName FROM TuitionInvoices i JOIN StudentDB.dbo.Students s ON i.StudentId = s.Id;
UPDATE i SET CourseNameSnapshot = c.Name FROM TuitionInvoices i JOIN CourseDB.dbo.Courses c ON i.CourseId = c.Id;
UPDATE i SET ClassNameSnapshot = c.ClassName FROM TuitionInvoices i JOIN CourseDB.dbo.Classes c ON i.ClassId = c.Id;
COMMIT;
"@)

Invoke-EduSql -Database "PaymentDB" -Query ($paymentLines -join [Environment]::NewLine)

Write-Host "Realistic EduCenter seed data applied."
