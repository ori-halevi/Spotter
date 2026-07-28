@echo off
rem Release build. Both modes produce ONE file - Spotter.exe and nothing else.
rem
rem   publish.bat        small exe, needs the .NET 8 Desktop Runtime installed
rem   publish.bat full   self-contained exe that runs on any Windows x64
rem
rem Labels rather than if-blocks on purpose: variables set inside a parenthesised block in cmd
rem are not visible in that same block without delayed expansion, which is a classic way to get
rem a script that silently publishes the wrong thing.
setlocal
cd /d "%~dp0"

if /i "%~1"=="full" goto full
if "%~1"=="" goto lite
echo Usage: publish.bat [full]
exit /b 1

:lite
set "OUT=publish\lite"
set "ARGS=--self-contained false"
set "NOTE=Needs the .NET 8 Desktop Runtime on the target PC."
goto run

:full
set "OUT=publish\full"
rem IncludeNativeLibrariesForSelfExtract: WPF ships native DLLs (wpfgfx, PresentationNative,
rem D3DCompiler...) which single-file publishing leaves beside the exe unless told otherwise.
rem Bundled, they are unpacked into a temp folder at first run instead - not into whatever
rem folder the user dropped Spotter in.
set "ARGS=--self-contained true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true"
set "NOTE=Self-contained. Runs on any Windows x64 with nothing preinstalled."
goto run

:run
echo Publishing to %OUT% ...
echo.
rmdir /s /q "%OUT%" 2>nul
dotnet publish src\Spotter\Spotter.csproj -c Release -r win-x64 -p:PublishSingleFile=true %ARGS% -o "%OUT%"
if errorlevel 1 goto fail

echo.
echo Published.
echo   %NOTE%
echo.
echo Contents of %OUT%:
dir /b "%OUT%"
echo.
echo That should be one file. Spotter keeps its settings in %%APPDATA%%\Spotter and its log in
echo %%LOCALAPPDATA%%\Spotter, so it writes nothing beside itself wherever you put it.
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
