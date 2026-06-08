# Demo Flow

1. Login admin:

```http
POST /gateway/auth/login
```

2. Copy `accessToken`.
3. Create course via `POST /gateway/courses`.
4. Create teacher/class/schedule via `/gateway/teachers`, `/gateway/classes`, `/gateway/schedules`.
5. Create student via `POST /gateway/students`.
6. Create enrollment via `POST /gateway/enrollments`, then confirm with `PUT /gateway/enrollments/{id}/confirm`.
7. Create tuition invoice via `POST /gateway/tuition`.
8. Login `student01 / Student@123`.
9. Student checks:

```txt
GET /gateway/students/{studentId}/learning-profile
GET /gateway/students/{studentId}/my-courses
GET /gateway/students/{studentId}/my-attendance
GET /gateway/students/{studentId}/my-results
GET /gateway/tuition/by-student/{studentId}
```

10. Login `teacher01 / Teacher@123`.
11. Teacher checks classes and schedules:

```txt
GET /gateway/classes/by-teacher/{teacherId}
GET /gateway/schedules/by-teacher/{teacherId}
```

12. Teacher creates attendance session:

```txt
POST /gateway/attendance/attendance-sessions
```

13. Teacher marks bulk attendance:

```txt
POST /gateway/attendance/attendance-records/bulk
```

14. Teacher creates result:

```txt
POST /gateway/results
```

15. Admin checks reports:

```txt
GET /gateway/reports/revenue/overview
GET /gateway/reports/dashboard
```
