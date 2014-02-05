@echo off
pushd "%~dp0"

echo Release build LessCoffee.csproj ...
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe ..\src\Handlers\LessCoffee.csproj /nologo /t:Build /p:Configuration=Release /verbosity:quiet

nuget pack LessCoffee.nuspec -version 2.5.6.0 -NoPackageAnalysis

