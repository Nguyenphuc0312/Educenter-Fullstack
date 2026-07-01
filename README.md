# EduCenter Fullstack

EduCenter la he thong quan ly trung tam giao duc gom day du cac phan he: khoa hoc, lop hoc, lich hoc, giang vien, hoc vien, ghi danh, diem danh, ket qua hoc tap, hoc phi, thanh toan, bao cao va tro ly AI.

Thu muc nay da dong goi ca frontend, backend, file chay he thong va file tao du lieu demo.

## Cau truc thu muc

```txt
Educenter-Fullstack-Package/
  backend/                         ASP.NET Core 8 microservices + API Gateway
  frontend/                        Vue 3 + Vite frontend
  scripts/
    setup-demo-database.ps1        Tao du lieu demo tren SQL Server local
    setup-demo-database-docker.ps1 Tao du lieu demo cho SQL Server Docker
  database-snapshot/               Snapshot .bak cua database local chuan
  run-all-local.cmd                Chay toan bo he thong voi SQL Server local
  run-demo-local.cmd               Chay demo voi SQL Server Docker rieng
  stop-local.cmd                   Dung cac service local
  stop-demo-local.cmd              Dung demo Docker/local
```

## Kien truc he thong

| Thanh phan | Cong nghe | Port | Database |
| --- | --- | ---: | --- |
| Frontend | Vue 3, Vite, Ant Design Vue | 5173 | - |
| API Gateway | ASP.NET Core 8, Ocelot | 5000 | - |
| CourseScheduleService | ASP.NET Core 8, EF Core | 5001 | CourseDB |
| StudentAttendanceService | ASP.NET Core 8, EF Core, AI Assistant | 5002 | StudentDB |
| PaymentReportService | ASP.NET Core 8, EF Core, JWT Auth | 5003 | PaymentDB |

Frontend goi API qua Gateway:

```txt
http://127.0.0.1:5000
```

## Yeu cau moi truong

Can cai dat:

- Windows 10/11.
- .NET SDK 8.
- Node.js 20+ va npm.
- Mot trong hai lua chon database:
  - SQL Server local/SQL Server Express/SQL Server Developer.
  - Docker Desktop de chay SQL Server demo container.

Kiem tra:

```bat
dotnet --version
node --version
npm --version
```

## Cau hinh AI Router

Neu muon dung tinh nang tro ly AI qua backend, dat bien moi truong:

```bat
set EDUCENTER_AI_ROUTER_API_KEY=your_api_key_here
```

Hoac tao file `.env.local` tu file mau `.env.local.example`:

```bat
copy .env.local.example .env.local
notepad .env.local
```

`run-all-local.cmd` va `run-demo-local.cmd` se tu nap file nay. Khong commit API key vao git.

## Cach tao du lieu demo

### Cach 0: Restore snapshot database local chuan

Thu muc `database-snapshot/` co san 3 file backup:

- `CourseDB.bak`
- `StudentDB.bak`
- `PaymentDB.bak`

Co the restore bang SQL Server Management Studio hoac xem huong dan trong `database-snapshot/README.md`.

### Cach 1: Tao du lieu tren SQL Server local

Chay trong thu muc `Educenter-Fullstack-Package`:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup-demo-database.ps1
```

Neu dung SQL Server Express:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup-demo-database.ps1 -ServerInstance "localhost\SQLEXPRESS"
```

Neu dung SQL Login:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup-demo-database.ps1 -ServerInstance "localhost" -SqlUser "sa" -SqlPassword "YourStrongPassword"
```

Script se tao va nap du lieu cho:

- `CourseDB`: khoa hoc, lop hoc, lich hoc, phong hoc, giang vien.
- `StudentDB`: hoc vien, ghi danh, diem danh, ket qua, tai lieu AI.
- `PaymentDB`: tai khoan, hoc phi, hoa don, thanh toan, cai dat, thong bao.

### Cach 2: Chay demo voi Docker SQL Server rieng

Neu khong muon dung SQL Server tren may, chay:

```bat
run-demo-local.cmd
```

Script nay se:

1. Khoi dong SQL Server Docker container `educenter-demo-sqlserver`.
2. Tao va nap database demo.
3. Build/chay backend bang Docker Compose.
4. Chay frontend tai `http://127.0.0.1:5173`.

## Cach chay project

### Chay voi SQL Server local

Trong thu muc `Educenter-Fullstack-Package`:

```bat
run-all-local.cmd
```

Mac dinh script dung Windows Authentication voi SQL Server `localhost`.

Neu may dung SQL Server Express:

```bat
set SQL_SERVER=localhost\SQLEXPRESS
run-all-local.cmd
```

Neu dung SQL Login:

```bat
set SQL_SERVER=localhost
set SQL_USER=sa
set SQL_PASSWORD=YourStrongPassword
run-all-local.cmd
```

Sau khi chay, mo:

```txt
Frontend:    http://127.0.0.1:5173
Gateway:     http://127.0.0.1:5000
Course API:  http://127.0.0.1:5001/swagger
Student API: http://127.0.0.1:5002/swagger
Payment API: http://127.0.0.1:5003/swagger
```

### Dung he thong

```bat
stop-local.cmd
```

Neu dang chay demo Docker:

```bat
stop-demo-local.cmd
```

## Tai khoan demo

```txt
Admin:     admin     / Admin@123
Giang vien: teacher01 / Teacher@123
Hoc vien:   student01 / Student@123
```

## Kiem tra build

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

## Huong dan demo va giai thich project

Khi trinh bay project, co the di theo thu tu sau:

1. Mo trang public, gioi thieu danh sach khoa hoc va dang ky tai khoan hoc vien.
2. Dang nhap Admin:
   - Quan ly khoa hoc, lop hoc, phong hoc, lich hoc.
   - Quan ly hoc vien, ghi danh, hoc phi, thanh toan.
   - Xem bao cao doanh thu va tinh hinh hoc tap.
   - Quan ly tri thuc AI.
3. Dang nhap Giang vien:
   - Xem lop phu trach.
   - Xem lich day theo tuan.
   - Tao phien diem danh.
   - Nhap ket qua hoc tap.
   - Dung tro ly AI de hoi lich, lop phu trach, soan giao an, goi y cau hoi.
4. Dang nhap Hoc vien:
   - Xem khoa hoc/lop hoc cua minh.
   - Xem lich hoc, diem danh, ket qua, hoc phi.
   - Danh gia lop hoc/giang vien.
5. Giai thich kien truc:
   - Frontend Vue goi API qua Gateway.
   - Gateway dieu huong sang 3 microservice.
   - Moi microservice co database rieng.
   - Du lieu lien service dung `Guid` reference va snapshot text, khong tao foreign key cheo database.

## Luu y khi nop va push code

- Khong commit `node_modules`, `dist`, `bin`, `obj`, log, backup local, key hoac file `.env.production`.
- Khong commit API key hoac mat khau that.
- Neu can thay doi SQL Server theo may, dung bien moi truong thay vi sua source code.
