@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo ============================================
echo EduCenter - Stop Docker demo
echo ============================================
echo.

echo Stopping frontend on port 5173 if running...
for /f "tokens=5" %%A in ('netstat -ano ^| findstr /R /C:":5173 .*LISTENING"') do (
  echo Stopping port 5173, PID %%A
  taskkill /PID %%A /F >nul 2>nul
)

echo.
echo Stopping Docker backend containers...
pushd backend
docker compose down
popd

echo.
echo Done.
pause
