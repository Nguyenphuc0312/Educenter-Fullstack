FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY StudentAttendanceService/StudentAttendanceService.csproj StudentAttendanceService/
RUN dotnet restore StudentAttendanceService/StudentAttendanceService.csproj
COPY StudentAttendanceService/ StudentAttendanceService/
RUN dotnet publish StudentAttendanceService/StudentAttendanceService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5002
ENTRYPOINT ["dotnet", "StudentAttendanceService.dll"]
