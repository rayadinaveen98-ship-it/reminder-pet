@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>&1
if errorlevel 1 (
  echo .NET 10 SDK not found. Install the free .NET 10 SDK and re-run this file.
  pause
  exit /b 1
)
echo Building PetPing for Windows x64. The SDK might download free NuGet dependencies.
dotnet publish "PetPing.Windows\PetPing.Windows.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
if errorlevel 1 (
  echo BUILD FAILED. Review the errors above.
  pause
  exit /b 1
)
echo.
echo Build files: PetPing.Windows\bin\Release\net10.0-windows\win-x64\publish\
echo Run PetPing.Windows.exe from that folder on Windows.
pause
