@echo off
pushd "%~dp0"

:: Get latest package
for %%F in (LessCoffee.*.nupkg) do set PACKAGE=%%F

nuget push "%PACKAGE%"
