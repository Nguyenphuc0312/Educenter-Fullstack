@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo ============================================
echo EduCenter - Local demo with isolated Docker DB
echo ============================================
echo.

where docker >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Docker is not installed or docker is not in PATH.
  pause
  exit /b 1
)

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

call "%~dp0load-local-env.cmd"

docker info >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Docker Desktop is not running.
  echo Open Docker Desktop, wait until it is ready, then run this file again.
  pause
  exit /b 1
)

echo Stopping frontend process on port 5173 if any...
call :free_port 5173
echo.

echo Starting demo SQL Server container...
pushd backend
docker compose -f docker-compose.demo.yml up -d sqlserver
if errorlevel 1 (
  popd
  echo [ERROR] Could not start demo SQL Server container.
  pause
  exit /b 1
)
popd

echo Waiting for SQL Server to accept connections...
for /L %%A in (1,1,60) do (
  docker exec educenter-demo-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "EduCenter@123456" -C -Q "SELECT 1" -b >nul 2>nul
  if not errorlevel 1 goto :sql_ready
  timeout /t 2 /nobreak >nul
)
echo [ERROR] Demo SQL Server was not ready after 120 seconds.
pause
exit /b 1

:sql_ready
echo SQL Server is ready.
echo.

echo Applying latest realistic demo data...
powershell -ExecutionPolicy Bypass -File ".\scripts\setup-demo-database-docker.ps1" -ServerInstance "127.0.0.1,14330" -SqlUser "sa" -SqlPassword "EduCenter@123456" -DockerSqlContainer "educenter-demo-sqlserver"
if errorlevel 1 (
  echo [ERROR] Demo database setup failed.
  pause
  exit /b 1
)

echo.
echo Starting backend services and API Gateway...
pushd backend
docker compose -f docker-compose.demo.yml up --build -d
if errorlevel 1 (
  popd
  echo [ERROR] docker compose up failed.
  pause
  exit /b 1
)
popd

if not exist "frontend\node_modules" (
  echo.
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

echo.
echo Starting frontend...
start "EduCenter Frontend :5173" cmd /k "cd /d ""%~dp0frontend""&& npm run dev -- --host 127.0.0.1 --port 5173"

echo.
echo Demo is starting. Open:
echo   Frontend:    http://127.0.0.1:5173
echo   API Gateway: http://127.0.0.1:5000
echo   Course API:  http://127.0.0.1:5001/swagger
echo   Student API: http://127.0.0.1:5002/swagger
echo   Payment API: http://127.0.0.1:5003/swagger
echo.
echo Login:
echo   admin     / Admin@123
echo   teacher01 / Teacher@123
echo   student01 / Student@123
echo.
echo To stop this local demo, run stop-demo-local.cmd.
pause
exit /b 0

:free_port
for /f "tokens=5" %%A in ('netstat -ano ^| findstr /R /C:":%~1 .*LISTENING"') do (
  echo Port %~1 is already in use. Stopping PID %%A...
  taskkill /PID %%A /F >nul 2>nul
)
exit /b 0
