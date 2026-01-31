@echo off
title Kodama Demo Launcher
echo ========================================
echo        KODAMA DEMO LAUNCHER
echo ========================================
echo.

:: Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK/Runtime not found!
    echo Please install .NET 8.0 from: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

echo [1/2] Starting Backend Server...
echo      (Keep this window open!)
echo.

:: Start backend in a new window
start "Kodama Backend" cmd /k "cd /d %~dp0 && dotnet run --project Kodama.API --configuration Release"

:: Wait for server to start
echo Waiting for server to initialize...
timeout /t 3 /nobreak >nul

echo.
echo [2/2] Starting Unity Client...
echo.

:: Check if built client exists
if exist "%~dp0Kodama.Client\Build\Kodama.exe" (
    start "" "%~dp0Kodama.Client\Build\Kodama.exe"
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
echo Demo is running!
echo.
echo - Backend: http://localhost:5059
echo - Press Ctrl+C in Backend window to stop
echo ========================================
echo.
pause
