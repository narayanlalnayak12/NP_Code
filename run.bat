@echo off
setlocal
cd /d "%~dp0"

call dotnet build "SelectPhotos.Desktop\SelectPhotos.Desktop.csproj" --configuration Release
if errorlevel 1 exit /b %errorlevel%

start "SelectPhotos Desktop" powershell -NoExit -Command "& { Set-Location -LiteralPath '%~dp0'; dotnet run --project 'SelectPhotos.Desktop\SelectPhotos.Desktop.csproj'; }"

echo.
echo SelectPhotos desktop app started.
exit /b 0
