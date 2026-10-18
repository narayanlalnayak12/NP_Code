$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$desktopProject = Join-Path $root "SelectPhotos.Desktop\SelectPhotos.Desktop.csproj"

Write-Host "==> Building desktop app"
Set-Location -LiteralPath $root
& dotnet build $desktopProject --configuration Release

Write-Host "==> Starting desktop app"
$desktopProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "& { Set-Location -LiteralPath '$root'; dotnet run --project '$desktopProject'; }" -PassThru

Write-Host "Desktop PID: $($desktopProcess.Id)"
Write-Host "The desktop app is running."
