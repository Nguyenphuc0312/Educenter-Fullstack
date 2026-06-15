# EduCenter Fullstack

Hệ thống quản lý trung tâm giáo dục: khóa học, lớp học, lịch học, học viên, ghi danh, điểm danh, kết quả học tập, học phí, thanh toán và báo cáo.

Repository gồm:

- `backend/`: ASP.NET Core 8 microservices và API Gateway.
- `frontend/`: Vue 3 + Vite UI.
- `run-all-local.cmd`: chạy toàn bộ hệ thống trên Windows.
- `stop-local.cmd`: dừng các tiến trình local theo port.

## Kiến trúc local

| Thành phần | Port | Database |
| --- | ---: | --- |
| API Gateway | `5000` | - |
| CourseScheduleService | `5001` | `CourseDB` |
| StudentAttendanceService | `5002` | `StudentDB` |
| PaymentReportService | `5003` | `PaymentDB` |
| Frontend | `5173` | - |

Frontend gọi API qua Gateway:

```txt
http://127.0.0.1:5000
```

## Yêu cầu môi trường

Cài trước:

- Windows 10/11.
- .NET SDK 8.
- Node.js 20+.
- SQL Server local, SQL Server Express, SQL Server Developer hoặc SQL Server container.

Kiểm tra nhanh:

```bat
dotnet --version
node --version
npm --version
```

## Cấu hình database cho từng máy

Backend dùng 3 database:

```txt
CourseDB
StudentDB
PaymentDB
```

Code sẽ tự tạo bảng và seed dữ liệu khi service chạy. Thành viên chỉ cần trỏ connection string đúng SQL Server của máy mình.

### Cách 1: Windows Authentication

Nếu SQL Server của bạn dùng Windows Authentication:

```bat
set SQL_SERVER=localhost
run-all-local.cmd
```

Nếu dùng SQL Server Express:

```bat
set SQL_SERVER=localhost\SQLEXPRESS
run-all-local.cmd
```

Nếu dùng instance khác, đổi `SQL_SERVER` theo máy của bạn.

### Cách 2: SQL Login

Nếu dùng tài khoản SQL, ví dụ `sa`:

```bat
set SQL_SERVER=localhost
set SQL_USER=sa
set SQL_PASSWORD=YourStrongPassword
run-all-local.cmd
```

Script sẽ tự truyền các biến sau cho backend, nên không cần sửa `appsettings.json`:

```txt
ConnectionStrings__CourseDB
ConnectionStrings__StudentDB
ConnectionStrings__PaymentDB
```

## Chạy dự án sau khi pull

Từ thư mục repo:

```bat
run-all-local.cmd
```

Script sẽ:

1. Kiểm tra `dotnet` và `node`.
2. Cài frontend dependencies nếu `frontend/node_modules` chưa có.
3. Dừng tiến trình đang chiếm các port `5000`, `5001`, `5002`, `5003`, `5173`.
4. Mở từng cửa sổ terminal cho 4 backend service và frontend.

Sau khi chạy, mở:

```txt
http://127.0.0.1:5173
```

API Gateway:

```txt
http://127.0.0.1:5000
```

## Dừng dự án

```bat
stop-local.cmd
```

Script sẽ kill các tiến trình đang listen trên:

```txt
5000, 5001, 5002, 5003, 5173
```

## Tài khoản test

```txt
admin     / Admin@123
teacher01 / Teacher@123
student01 / Student@123
```

## Chạy thủ công nếu cần debug

Mở 5 terminal:

```bat
cd backend\CourseScheduleService
set ASPNETCORE_URLS=http://127.0.0.1:5001
dotnet run --no-launch-profile
```

```bat
cd backend\StudentAttendanceService
set ASPNETCORE_URLS=http://127.0.0.1:5002
dotnet run --no-launch-profile
```

```bat
cd backend\PaymentReportService
set ASPNETCORE_URLS=http://127.0.0.1:5003
dotnet run --no-launch-profile
```

```bat
cd backend\ApiGateway
set ASPNETCORE_URLS=http://127.0.0.1:5000
dotnet run --no-launch-profile
```

```bat
cd frontend
npm install
npm run dev -- --host 127.0.0.1 --port 5173
```

Nếu chạy thủ công trên máy khác, cần tự set connection string hoặc sửa tạm `backend/*Service/appsettings.json`.

## Build kiểm tra

Backend:

```bat
dotnet build backend\EduCenter.Backend.slnx
```

Frontend:

```bat
cd frontend
npm install
npm run build
```

## Lỗi thường gặp

### Network Error khi đăng nhập

Kiểm tra Gateway có chạy không:

```txt
http://127.0.0.1:5000/gateway/auth/login
```

Frontend local phải dùng:

```env
VITE_API_BASE_URL=http://127.0.0.1:5000
```

### Không kết nối được SQL Server

Đặt lại `SQL_SERVER` trước khi chạy:

```bat
set SQL_SERVER=localhost\SQLEXPRESS
run-all-local.cmd
```

Hoặc dùng SQL Login:

```bat
set SQL_SERVER=localhost
set SQL_USER=sa
set SQL_PASSWORD=YourStrongPassword
run-all-local.cmd
```

### Port đã bị chiếm

Chạy:

```bat
stop-local.cmd
```

Rồi chạy lại:

```bat
run-all-local.cmd
```

## Ghi chú cho thành viên

- Không commit `node_modules`, `bin`, `obj`, `dist`, log hoặc file backup cá nhân.
- Không hard-code tên SQL Server cá nhân vào source khi commit.
- Nếu cần đổi connection string, ưu tiên dùng biến môi trường như hướng dẫn ở trên.
