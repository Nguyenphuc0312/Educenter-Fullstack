@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-educenter-service.ps1" -Name student -WorkingDir "%~dp0backend\StudentAttendanceService" -Url "http://127.0.0.1:5002"
