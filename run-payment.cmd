@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-educenter-service.ps1" -Name payment -WorkingDir "%~dp0backend\PaymentReportService" -Url "http://127.0.0.1:5003"
