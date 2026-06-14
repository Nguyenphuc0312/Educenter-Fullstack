@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-educenter-service.ps1" -Name gateway -WorkingDir "%~dp0backend\ApiGateway" -Url "http://127.0.0.1:5000"
