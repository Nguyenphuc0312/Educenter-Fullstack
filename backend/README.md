# EduCenter Backend

Backend microservices cho đề tài 04: Hệ thống quản lý khóa học và học viên trung tâm đào tạo.

## Kiến trúc

```txt
EduCenter.Backend/
  ApiGateway/                 ASP.NET Core + Ocelot, port 5000
  CourseScheduleService/      CourseDB, port 5001
  StudentAttendanceService/   StudentDB, port 5002
  PaymentReportService/       PaymentDB, port 5003
  docker-compose.yml
```

Nguyên tắc: mỗi service có database riêng, không foreign key chéo service. Dữ liệu service khác được lưu bằng `Guid` reference và snapshot text như `CourseNameSnapshot`, `ClassNameSnapshot`, `StudentNameSnapshot`.

## Chạy bằng Docker

```bash
cd EduCenter.Backend
docker compose up --build
```

Ports:

```txt
ApiGateway:              http://localhost:5000
CourseScheduleService:   http://localhost:5001/swagger
StudentAttendanceService:http://localhost:5002/swagger
PaymentReportService:   http://localhost:5003/swagger
CourseDB:                localhost,14331
StudentDB:               localhost,14332
PaymentDB:               localhost,14333
```

SQL Server password demo:

```txt
sa / EduCenter@123456
```

## Chạy từng service local

Cần SQL Server đang chạy ở các port 14331, 14332, 14333 hoặc sửa connection string trong `appsettings.json`.

```bash
dotnet run --project CourseScheduleService --urls http://localhost:5001
dotnet run --project StudentAttendanceService --urls http://localhost:5002
dotnet run --project PaymentReportService --urls http://localhost:5003
```

Database được tạo và seed tự động bằng `EnsureCreated`.

`ApiGateway/ocelot.json` đang trỏ tới Docker service names `course-service`, `student-service`, `payment-service`, nên gateway nên chạy bằng `docker compose`. Nếu muốn chạy gateway trực tiếp ngoài Docker, đổi các `DownstreamHostAndPorts.Host` trong `ApiGateway/ocelot.json` thành `localhost`.

## Tài khoản seed

```txt
admin     / Admin@123   / Admin
teacher01 / Teacher@123 / Teacher
teacher02 / Teacher@123 / Teacher
student01 / Student@123 / Student
student02 / Student@123 / Student
```

Login:

```http
POST http://localhost:5003/api/auth/login
POST http://localhost:5000/gateway/auth/login
```

Body:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

## Endpoint chính

Public:

```txt
GET  /gateway/courses
GET  /gateway/courses/opening
GET  /gateway/classes/opening
GET  /gateway/teachers
POST /gateway/auth/login
POST /gateway/auth/register
```

Private qua Gateway:

```txt
/gateway/students/**
/gateway/enrollments/**
/gateway/attendance/**
/gateway/results/**
/gateway/accounts/**
/gateway/tuition/**
/gateway/payments/**
/gateway/reports/**
```

Gửi JWT:

```txt
Authorization: Bearer <accessToken>
```

## Response format

Tất cả service trả cùng format:

```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "errors": null
}
```

Lỗi validation:

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    { "field": "name", "message": "Name is required" }
  ]
}
```

## Ghi chú

- Swagger có Bearer authorize button ở cả 3 service chính.
- JWT được cấp bởi `PaymentReportService`.
- Gateway và service đều validate cùng issuer/audience/secret.
- Code dùng Controller - Service - Repository - DbContext.
- Seed data đủ cho demo khóa học, lớp, lịch học, học viên, điểm danh, kết quả, học phí và báo cáo.
