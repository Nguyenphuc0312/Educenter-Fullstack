# API Contract

## Common Response

```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "errors": null
}
```

Paged data:

```json
{
  "success": true,
  "message": "OK",
  "data": {
    "items": [],
    "pageIndex": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  },
  "errors": null
}
```

## Auth

`POST /api/auth/login`

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

JWT claims:

```txt
sub, userId, username, role, referenceId
```

## Gateway Prefixes

```txt
/gateway/courses      -> CourseScheduleService /api/courses
/gateway/classes      -> CourseScheduleService /api/classes
/gateway/schedules    -> CourseScheduleService /api/schedules
/gateway/teachers     -> CourseScheduleService /api/teachers
/gateway/students     -> StudentAttendanceService /api/students
/gateway/enrollments  -> StudentAttendanceService /api/enrollments
/gateway/attendance   -> StudentAttendanceService /api/*
/gateway/results      -> StudentAttendanceService /api/results
/gateway/auth         -> PaymentReportService /api/auth
/gateway/accounts     -> PaymentReportService /api/accounts
/gateway/tuition      -> PaymentReportService /api/tuition-invoices
/gateway/payments     -> PaymentReportService /api/payments
/gateway/reports      -> PaymentReportService /api/reports
```

## Roles

```txt
Admin: full management
Teacher: class, attendance, results
Student: own learning, attendance, tuition, results
Anonymous: public course/auth routes
```
