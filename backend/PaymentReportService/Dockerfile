FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PaymentReportService/PaymentReportService.csproj PaymentReportService/
RUN dotnet restore PaymentReportService/PaymentReportService.csproj
COPY PaymentReportService/ PaymentReportService/
RUN dotnet publish PaymentReportService/PaymentReportService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5003
ENTRYPOINT ["dotnet", "PaymentReportService.dll"]
