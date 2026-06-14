@echo off
setlocal
cd /d "%~dp0"

echo Starting EduCenter backend services and frontend...
echo.

start "EduCenter - CourseScheduleService :5001" cmd /k "%~dp0run-course.cmd"
timeout /t 2 /nobreak >nul

start "EduCenter - StudentAttendanceService :5002" cmd /k "%~dp0run-student.cmd"
timeout /t 2 /nobreak >nul

start "EduCenter - PaymentReportService :5003" cmd /k "%~dp0run-payment.cmd"
timeout /t 2 /nobreak >nul

start "EduCenter - ApiGateway :5000" cmd /k "%~dp0run-gateway.cmd"
timeout /t 2 /nobreak >nul

start "EduCenter - Frontend :5173" cmd /k "%~dp0run-frontend.cmd"

echo.
echo Frontend:    http://127.0.0.1:5173
echo API Gateway: http://127.0.0.1:5000
echo.
echo Close the opened windows or run stop-local.cmd to stop the project.
pause
