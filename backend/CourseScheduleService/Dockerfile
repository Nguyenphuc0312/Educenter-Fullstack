FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CourseScheduleService/CourseScheduleService.csproj CourseScheduleService/
RUN dotnet restore CourseScheduleService/CourseScheduleService.csproj
COPY CourseScheduleService/ CourseScheduleService/
RUN dotnet publish CourseScheduleService/CourseScheduleService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "CourseScheduleService.dll"]
