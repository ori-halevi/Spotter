@echo off
rem Builds BOTH release flavours and leaves exactly two files in publish\, named so that anyone
rem looking at a GitHub release page can tell which one they want without reading anything:
rem
rem   Spotter-<ver>-win-x64-standalone.exe        everything inside, no prerequisites
rem   Spotter-<ver>-win-x64-requires-dotnet8.exe  small, needs the .NET 8 Desktop Runtime
rem
rem Each is a single file. Spotter writes nothing beside itself wherever it is put.
setlocal
cd /d "%~dp0"

rem Version comes from the csproj so the file names can never drift from what the exe reports.
rem findstr matches the literal "<Version>", which does not appear inside <AssemblyVersion> or
rem <FileVersion>, so this picks exactly one line.
set "VER="
for /f "tokens=3 delims=<>" %%v in ('findstr /i /c:"<Version>" src\Spotter\Spotter.csproj') do set "VER=%%v"
if not defined VER (
    echo Could not read ^<Version^> from src\Spotter\Spotter.csproj.
    pause
    exit /b 1
)

set "OUT=publish"
set "TMP_=%OUT%\_build"
set "STANDALONE=Spotter-%VER%-win-x64-standalone.exe"
set "NEEDSNET=Spotter-%VER%-win-x64-requires-dotnet8.exe"

echo Spotter %VER% - building both release flavours
echo.

rmdir /s /q "%OUT%" 2>nul
mkdir "%TMP_%" 2>nul

echo [1/2] Standalone (self-contained, no prerequisites)...
rem IncludeNativeLibrariesForSelfExtract: WPF ships native DLLs (wpfgfx, PresentationNative,
rem D3DCompiler...) which single-file publishing otherwise leaves beside the exe. Bundled, they
rem are unpacked into a temp folder at first run instead of into wherever Spotter was dropped.
dotnet publish src\Spotter\Spotter.csproj -c Release -r win-x64 ^
    -p:PublishSingleFile=true --self-contained true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o "%TMP_%\standalone"
if errorlevel 1 goto fail
move /y "%TMP_%\standalone\Spotter.exe" "%OUT%\%STANDALONE%" >nul
if errorlevel 1 goto fail

echo.
echo [2/2] Framework-dependent (needs the .NET 8 Desktop Runtime)...
dotnet publish src\Spotter\Spotter.csproj -c Release -r win-x64 ^
    -p:PublishSingleFile=true --self-contained false ^
    -o "%TMP_%\lite"
if errorlevel 1 goto fail
move /y "%TMP_%\lite\Spotter.exe" "%OUT%\%NEEDSNET%" >nul
if errorlevel 1 goto fail

rmdir /s /q "%TMP_%" 2>nul

echo.
echo ============================================================
echo  Release %VER% ready in %OUT%\
echo ============================================================
for %%f in ("%OUT%\*.exe") do echo   %%~nxf   %%~zf bytes
echo.
echo  Upload BOTH to the same GitHub release.
echo    standalone        - runs on any Windows x64, nothing to install
echo    requires-dotnet8  - small; needs the .NET 8 Desktop Runtime
echo.
pause
exit /b 0

:fail
echo.
echo PUBLISH FAILED.
echo.
echo If the error is a missing Spotter.Core project, see the README: the search engine
echo lives in engine\ and is deliberately not part of this repository.
echo.
pause
exit /b 1
