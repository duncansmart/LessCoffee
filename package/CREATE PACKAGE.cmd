@echo off
pushd "%~dp0"

:: Get and increment BUILDVER from buildver.txt
for /F %%V in (buildver.txt) do set /a BUILDVER=%%V + 1

echo BUILDVER='%BUILDVER%'

nuget pack LessCoffee.nuspec -version 2.0.0.%BUILDVER%
if %ERRORLEVEL% == 0 (
    echo %BUILDVER% > buildver.txt
)
