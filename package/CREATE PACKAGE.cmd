@echo off
pushd "%~dp0"

:: Get and increment BUILDVER from buildver.txt
::for /F %%V in (buildver.txt) do set /a BUILDVER=%%V + 1
::echo BUILDVER='%BUILDVER%'
set /a BUILDVER=0

nuget pack LessCoffee.nuspec -version 2.1.2.%BUILDVER%
if %ERRORLEVEL% == 0 (
    echo %BUILDVER% > buildver.txt
)
