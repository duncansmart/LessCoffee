@echo off
pushd "%~dp0\nodejs"
set zip="%ProgramFiles%\7-Zip\7z.exe"
if not exist %zip% (
  echo ERROR: Please install the 32-bit version of 7-zip
  exit /b 1
)
%zip% a -r %temp%\nodejs.tar *
%zip% a ..\Resources\nodejs.tgz %temp%\nodejs.tar
popd