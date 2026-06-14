@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-educenter-service.ps1" -Name course -WorkingDir "%~dp0backend\CourseScheduleService" -Url "http://127.0.0.1:5001"
