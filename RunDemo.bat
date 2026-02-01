@echo off
title Kodama Demo Launcher
echo ========================================
echo        KODAMA DEMO LAUNCHER
echo ========================================
echo.

echo [1/2] Starting Backend Server...
echo      (Keep this window open!)
echo.

:: Start backend - check multiple locations
if exist "%~dp0Backend\Kodama.API.exe" (
    :: Packaged release structure
    start "Kodama Backend" cmd /k "cd /d %~dp0Backend && Kodama.API.exe"
) else if exist "%~dp0Publish\Kodama.API.exe" (
    :: Development publish structure
    start "Kodama Backend" cmd /k "cd /d %~dp0Publish && Kodama.API.exe"
) else (
    :: Fallback to dotnet run
    dotnet --version >nul 2>&1
    if %errorlevel% neq 0 (
        echo [ERROR] Backend not found and .NET SDK not installed!
        echo Please run Build.bat first or install .NET 8.0
        pause
        exit /b 1
    )
    start "Kodama Backend" cmd /k "cd /d %~dp0 && dotnet run --project Kodama.API --configuration Release"
)

:: Wait for server to start (check port 5000)
echo Waiting for server to initialize...
:wait_loop
powershell -Command "try { $null = [System.Net.Sockets.TcpClient]::new('localhost', 5000); exit 0 } catch { exit 1 }" >nul 2>&1
if %errorlevel% neq 0 (
    timeout /t 1 /nobreak >nul
    goto wait_loop
)
echo Server is ready!

echo.
echo [2/2] Starting Unity Client...
echo.

:: Check if built client exists - check multiple locations
if exist "%~dp0Client\Kodama.Client.exe" (
    :: Packaged release structure
    start "" "%~dp0Client\Kodama.Client.exe"
) else if exist "%~dp0Kodama.Client\Build\Kodama.Client.exe" (
    :: Development structure
    start "" "%~dp0Kodama.Client\Build\Kodama.Client.exe"
) else (
    echo [WARNING] Unity client build not found!
    echo.
    echo To run the client:
    echo   1. Open Unity Hub
    echo   2. Open project: Kodama.Client
    echo   3. Open scene: Assets/Scenes/Main.unity
    echo   4. Press Play
    echo.
    echo Or build the client:
    echo   File -^> Build Settings -^> Build to Kodama.Client/Build/
)

echo.
echo ========================================
echo Kodama is running!
echo.
echo - Backend: http://localhost:5000
echo - Press Ctrl+C in Backend window to stop
echo ========================================
echo.
pause
