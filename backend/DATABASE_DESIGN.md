# Database Design

## CourseDB

Tables:

```txt
Courses
Teachers
Classes
Schedules
```

Internal FK only:

```txt
Classes.CourseId -> Courses.Id
Classes.TeacherId -> Teachers.Id
Schedules.ClassId -> Classes.Id
```

## StudentDB

Tables:

```txt
Students
Enrollments
AttendanceSessions
AttendanceRecords
StudentResults
```

`CourseId`, `ClassId`, `ScheduleId`, `TeacherId` are plain references to other services. No cross-database FK.

## PaymentDB

Tables:

```txt
UserAccounts
TuitionInvoices
PaymentTransactions
RevenueReportSnapshots
```

`StudentId`, `CourseId`, `ClassId`, `ReferenceId` are plain references. Passwords are PBKDF2 hashes.
