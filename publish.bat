@echo off
rem Release build for handing to someone else.
rem
rem   publish.bat        small build, needs the .NET 8 Desktop Runtime installed
rem   publish.bat full   one self-contained exe that runs on any Windows x64
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
set "ARGS=--self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true"
set "NOTE=Self-contained. Runs on any Windows x64 with nothing preinstalled."
goto run

:run
echo Publishing to %OUT% ...
echo.
rmdir /s /q "%OUT%" 2>nul
dotnet publish src\Spotter\Spotter.csproj -c Release -r win-x64 %ARGS% -o "%OUT%"
if errorlevel 1 goto fail

echo.
echo Published.
echo   %OUT%\Spotter.exe
echo   %NOTE%
echo.
echo Reading the file table needs administrator; Spotter asks for it on first launch and
echo registers a scheduled task so later launches from the same path do not prompt again.
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
