@echo off
echo Building GetCastSchedule (Self-contained, Single-file)...
dotnet publish GetCastSchedule/GetCastSchedule.csproj -c Release
if %errorlevel% neq 0 (
    echo.
    echo Error: Build failed.
    pause
    exit /b %errorlevel%
)
echo.
echo Build successful!
echo Output directory: GetCastSchedule\bin\Release\net6.0\win-x64\publish\
echo.
pause
