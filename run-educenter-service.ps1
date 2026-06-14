param(
  [Parameter(Mandatory=$true)][string]$Name,
  [Parameter(Mandatory=$true)][string]$WorkingDir,
  [Parameter(Mandatory=$true)][string]$Url
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$logDir = Join-Path $root "run-logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
Set-Content -Path (Join-Path $logDir "$Name.host.pid") -Value $PID

Set-Location $WorkingDir
dotnet run --no-restore --no-launch-profile --urls $Url *>&1 |
  Tee-Object -FilePath (Join-Path $logDir "$Name.log")
