@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo ============================================
echo EduCenter - Run demo with Docker backend
echo ============================================
echo.

where docker >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Docker is not installed or docker is not in PATH.
  pause
  exit /b 1
)

where node >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Node.js is not installed or node is not in PATH.
  pause
  exit /b 1
)

docker info >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Docker Desktop is not running.
  echo Please open Docker Desktop, wait until it is ready, then run this file again.
  pause
  exit /b 1
)

echo Stopping frontend process on port 5173 if any...
call :free_port 5173
echo.

echo Starting backend services and SQL Server containers...
pushd backend
docker compose up --build -d
if errorlevel 1 (
  popd
  echo [ERROR] docker compose up failed.
  echo.
  echo If the error mentions dockerDesktopLinuxEngine or docker_engine:
  echo   1. Open Docker Desktop.
  echo   2. Wait until Docker Desktop is fully running.
  echo   3. Run this file again.
  echo.
  echo If the error says a port is already allocated:
  echo   Run stop-demo-docker.cmd, or stop the local app currently using ports 5000-5003.
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
echo To stop the demo, run stop-demo-docker.cmd.
pause
exit /b 0

:free_port
for /f "tokens=5" %%A in ('netstat -ano ^| findstr /R /C:":%~1 .*LISTENING"') do (
  echo Port %~1 is already in use. Stopping PID %%A...
  taskkill /PID %%A /F >nul 2>nul
)
exit /b 0
