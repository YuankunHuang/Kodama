@echo off
setlocal EnableDelayedExpansion

:: ========================================
::           CONFIGURATION
:: ========================================
set UNITY_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.3.1f1\Editor\Unity.exe
set UNITY_PROJECT=Kodama.Client
set UNITY_BUILD_METHOD=YuankunHuang.Kodama.Editor.BuildScript.BuildWindows
set VERSION=0.1.0

:: Unity Build Options
set SCRIPTING_BACKEND=IL2CPP :: IL2CPP, Mono
set STRIPPING_LEVEL=High :: Minimal, Low, Medium, High
set DEVELOPMENT_BUILD=false :: true, false
set COMPRESSION=LZ4HC :: LZ4, LZ4HC

:: ========================================
title Kodama Builder ^& Packer
echo ========================================
echo        KODAMA BUILDER ^& PACKER
echo ========================================
echo.

:: Record total start time
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set TOTAL_START=%%i

:: Close Unity if running
tasklist /FI "IMAGENAME eq Unity.exe" 2>NUL | find /I "Unity.exe" >NUL
if %errorlevel% equ 0 (
    echo Closing Unity Editor...
    taskkill /IM Unity.exe /F >NUL 2>&1
    timeout /t 3 /nobreak >NUL
)
echo.

:: Step 1: Backend
echo [1/3] Publishing Kodama.API...
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set STEP1_START=%%i
dotnet publish Kodama.API -c Release -r win-x64 --self-contained true -o ./Publish
if %errorlevel% neq 0 (
    echo ERROR: Backend build failed!
    pause
    exit /b 1
)
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set STEP1_END=%%i
set /a STEP1_TIME=STEP1_END-STEP1_START
echo Backend build complete. [%STEP1_TIME%s]
echo.

:: Step 2: Unity Client
echo [2/3] Building Unity Client...
:: Clear IL2CPP cache to ensure fresh build
if exist "%UNITY_PROJECT%\Library\Il2cppBuildCache" rmdir /s /q "%UNITY_PROJECT%\Library\Il2cppBuildCache"
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set STEP2_START=%%i
"%UNITY_EDITOR%" -quit -batchmode -nographics -projectPath "%UNITY_PROJECT%" -executeMethod %UNITY_BUILD_METHOD% -buildVersion %VERSION% -scriptingBackend %SCRIPTING_BACKEND% -strippingLevel %STRIPPING_LEVEL% -developmentBuild %DEVELOPMENT_BUILD% -compression %COMPRESSION% -logFile unity_build.log
if %errorlevel% neq 0 (
    echo ERROR: Unity build failed! Check unity_build.log
    pause
    exit /b 1
)
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set STEP2_END=%%i
set /a STEP2_TIME=STEP2_END-STEP2_START
echo Unity build complete. [%STEP2_TIME%s]
echo.

:: Step 3: Package
echo [3/3] Packaging release...
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set STEP3_START=%%i
for /f %%i in ('powershell -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set TIMESTAMP=%%i
set ZIP_NAME=Publish\Kodama-v%VERSION%-%TIMESTAMP%-win64.zip

:: Create clean package structure
set PKG_DIR=Publish\_package
if exist "%PKG_DIR%" rmdir /s /q "%PKG_DIR%"
mkdir "%PKG_DIR%"
mkdir "%PKG_DIR%\Backend"
mkdir "%PKG_DIR%\Client"

:: Copy files
echo Copying RunDemo.bat...
copy RunDemo.bat "%PKG_DIR%\" >NUL

echo Copying Backend files...
for %%f in (Publish\*.exe Publish\*.dll Publish\*.json) do copy "%%f" "%PKG_DIR%\Backend\" >NUL 2>&1

echo Copying Client files from %UNITY_PROJECT%\Build...
:: Copy all root-level files from Unity build (exe, dll, etc.)
for %%f in ("%UNITY_PROJECT%\Build\*.*") do (
    if not "%%~xf"=="" (
        echo   Copying %%~nxf
        copy "%%f" "%PKG_DIR%\Client\" >NUL 2>&1
    )
)

:: Copy required directories
echo Copying Kodama.Client_Data...
if exist "%UNITY_PROJECT%\Build\Kodama.Client_Data" (
    xcopy /E /I /Q "%UNITY_PROJECT%\Build\Kodama.Client_Data" "%PKG_DIR%\Client\Kodama.Client_Data\" >NUL
) else (
    echo WARNING: Kodama.Client_Data not found!
)

if exist "%UNITY_PROJECT%\Build\MonoBleedingEdge" (
    echo Copying MonoBleedingEdge...
    xcopy /E /I /Q "%UNITY_PROJECT%\Build\MonoBleedingEdge" "%PKG_DIR%\Client\MonoBleedingEdge\" >NUL
)

:: Create zip
powershell -Command "Compress-Archive -Path '%PKG_DIR%\*' -DestinationPath '%ZIP_NAME%' -Force"
if %errorlevel% neq 0 (
    echo ERROR: Packaging failed!
    pause
    exit /b 1
)

:: Clean up
rmdir /s /q "%PKG_DIR%"
for %%f in (Publish\*.exe Publish\*.dll Publish\*.json Publish\*.config Publish\*.pdb) do del "%%f" 2>NUL

for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set STEP3_END=%%i
set /a STEP3_TIME=STEP3_END-STEP3_START
echo Packaging complete. [%STEP3_TIME%s]
echo.

:: Calculate total time
for /f %%i in ('powershell -Command "[int](Get-Date -UFormat %%s)"') do set TOTAL_END=%%i
set /a TOTAL_TIME=TOTAL_END-TOTAL_START

echo ========================================
echo   BUILD COMPLETE: %ZIP_NAME%
echo ========================================
echo   Backend:   %STEP1_TIME%s
echo   Unity:     %STEP2_TIME%s
echo   Package:   %STEP3_TIME%s
echo   ----------------------
echo   Total:     %TOTAL_TIME%s
echo ========================================
pause