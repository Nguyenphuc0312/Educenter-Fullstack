@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo ============================================
echo EduCenter - Stop local Docker demo
echo ============================================
echo.

echo Stopping frontend on port 5173 if running...
for /f "tokens=5" %%A in ('netstat -ano ^| findstr /R /C:":5173 .*LISTENING"') do (
  echo Stopping port 5173, PID %%A
  taskkill /PID %%A /F >nul 2>nul
)

echo.
echo Stopping demo backend containers...
pushd backend
docker compose -f docker-compose.demo.yml down
popd

echo.
echo Done.
pause
