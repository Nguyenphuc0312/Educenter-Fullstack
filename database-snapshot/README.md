# EduCenter Database Snapshot

Thu muc nay chua snapshot database local chuan cua project:

- `CourseDB.bak`
- `StudentDB.bak`
- `PaymentDB.bak`

## Restore bang SQL Server Management Studio

1. Mo SQL Server Management Studio.
2. Chuot phai `Databases` -> `Restore Database...`.
3. Chon `Device` -> file `.bak` tu thu muc nay.
4. Restore lan luot 3 database voi dung ten:
   - `CourseDB`
   - `StudentDB`
   - `PaymentDB`

## Restore bang T-SQL

Sua duong dan file `.mdf/.ldf` cho dung voi may dang chay SQL Server.

```sql
RESTORE DATABASE CourseDB
FROM DISK = N'D:\Hoctap\Nam_3\Ki_3\full_stack\Educenter-Fullstack-Package\database-snapshot\CourseDB.bak'
WITH REPLACE;

RESTORE DATABASE StudentDB
FROM DISK = N'D:\Hoctap\Nam_3\Ki_3\full_stack\Educenter-Fullstack-Package\database-snapshot\StudentDB.bak'
WITH REPLACE;

RESTORE DATABASE PaymentDB
FROM DISK = N'D:\Hoctap\Nam_3\Ki_3\full_stack\Educenter-Fullstack-Package\database-snapshot\PaymentDB.bak'
WITH REPLACE;
```

Neu SQL Server bao loi duong dan data/log, dung `RESTORE FILELISTONLY FROM DISK = ...` de lay logical name roi restore voi `MOVE`.
