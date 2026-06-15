@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo ============================================
echo EduCenter - Run all services locally
echo ============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERROR] .NET SDK 8 is not installed or dotnet is not in PATH.
  pause
  exit /b 1
)

where node >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Node.js is not installed or node is not in PATH.
  pause
  exit /b 1
)

if not defined SQL_SERVER set "SQL_SERVER=localhost"

if defined SQL_USER (
  if not defined SQL_PASSWORD (
    echo [ERROR] SQL_USER is set but SQL_PASSWORD is missing.
    echo Example: set SQL_PASSWORD=YourPassword
    pause
    exit /b 1
  )
  set "COURSE_DB=Server=%SQL_SERVER%;Database=CourseDB;User Id=%SQL_USER%;Password=%SQL_PASSWORD%;TrustServerCertificate=True;Encrypt=False"
  set "STUDENT_DB=Server=%SQL_SERVER%;Database=StudentDB;User Id=%SQL_USER%;Password=%SQL_PASSWORD%;TrustServerCertificate=True;Encrypt=False"
  set "PAYMENT_DB=Server=%SQL_SERVER%;Database=PaymentDB;User Id=%SQL_USER%;Password=%SQL_PASSWORD%;TrustServerCertificate=True;Encrypt=False"
) else (
  set "COURSE_DB=Server=%SQL_SERVER%;Database=CourseDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
  set "STUDENT_DB=Server=%SQL_SERVER%;Database=StudentDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
  set "PAYMENT_DB=Server=%SQL_SERVER%;Database=PaymentDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
)

echo SQL Server: %SQL_SERVER%
if defined SQL_USER (
  echo SQL Login:  %SQL_USER%
) else (
  echo SQL Login:  Windows Authentication
)
echo.

call :free_port 5000
call :free_port 5001
call :free_port 5002
call :free_port 5003
call :free_port 5173

if not exist "frontend\node_modules" (
  echo Installing frontend dependencies...
  pushd frontend
  call npm install
  if errorlevel 1 (
    popd
    echo [ERROR] npm install failed.
    pause
    exit /b 1
  )
  popd
)

echo Starting backend services and frontend...
echo.

start "EduCenter CourseScheduleService :5001" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://127.0.0.1:5001&& set ConnectionStrings__CourseDB=%COURSE_DB%&& cd /d ""%~dp0backend\CourseScheduleService""&& dotnet run --no-launch-profile"
timeout /t 3 /nobreak >nul

start "EduCenter StudentAttendanceService :5002" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://127.0.0.1:5002&& set ConnectionStrings__StudentDB=%STUDENT_DB%&& cd /d ""%~dp0backend\StudentAttendanceService""&& dotnet run --no-launch-profile"
timeout /t 3 /nobreak >nul

start "EduCenter PaymentReportService :5003" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://127.0.0.1:5003&& set ConnectionStrings__PaymentDB=%PAYMENT_DB%&& cd /d ""%~dp0backend\PaymentReportService""&& dotnet run --no-launch-profile"
timeout /t 3 /nobreak >nul

start "EduCenter ApiGateway :5000" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://127.0.0.1:5000&& cd /d ""%~dp0backend\ApiGateway""&& dotnet run --no-launch-profile"
timeout /t 2 /nobreak >nul

start "EduCenter Frontend :5173" cmd /k "cd /d ""%~dp0frontend""&& npm run dev -- --host 127.0.0.1 --port 5173"

echo.
echo Started. Open these URLs:
echo   Frontend:    http://127.0.0.1:5173
echo   API Gateway: http://127.0.0.1:5000
echo.
echo Login:
echo   admin     / Admin@123
echo   teacher01 / Teacher@123
echo   student01 / Student@123
echo.
echo To stop everything, run stop-local.cmd.
pause
exit /b 0

:free_port
for /f "tokens=5" %%A in ('netstat -ano ^| findstr /R /C:":%~1 .*LISTENING"') do (
  echo Port %~1 is already in use. Stopping PID %%A...
  taskkill /PID %%A /F >nul 2>nul
)
exit /b 0
