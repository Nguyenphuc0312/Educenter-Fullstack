@echo off
setlocal enabledelayedexpansion

for %%P in (5000 5001 5002 5003 5173) do (
  for /f "tokens=5" %%A in ('netstat -ano ^| findstr /R /C:":%%P .*LISTENING"') do (
    echo Stopping port %%P, PID %%A
    taskkill /PID %%A /F >nul 2>nul
  )
)

echo Done.
pause
