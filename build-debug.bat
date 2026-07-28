@echo off
rem Debug build. Fast, unoptimised, full symbols - what you attach a debugger to.
setlocal
cd /d "%~dp0"

echo Building Spotter (Debug)...
echo.
dotnet build src\Spotter\Spotter.csproj -c Debug
if errorlevel 1 goto fail

echo.
echo Build succeeded.
echo   src\Spotter\bin\Debug\net8.0-windows\Spotter.exe
echo.
echo Useful switches when running it:
echo   --demo 250000   fill the grid with fake rows, no admin and no engine needed
echo   --noelevate     stay an ordinary process, so it can be focused and killed normally
echo   --lang he       force a language for one run without changing the saved setting
echo.
pause
exit /b 0

:fail
echo.
echo BUILD FAILED.
echo.
echo If the error is a missing Spotter.Core project, see the README: the search engine
echo lives in engine\ and is deliberately not part of this repository.
echo.
pause
exit /b 1
