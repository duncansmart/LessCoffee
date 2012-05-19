@echo off
pushd "%~dp0\nodejs"

set zip="%ProgramFiles%\7-Zip\7z.exe"
if not exist %zip% (
  echo ERROR: Please install the 32-bit version of 7-zip
  exit /b 1
)

if exist %temp%\nodejs.tar del %temp%\nodejs.tar
if exist ..\Resources\nodejs.tgz del ..\Resources\nodejs.tgz

%zip% a -r %temp%\nodejs.tar * ^
    -xr!node_modules\npm\ ^
    -xr!node_modules\less\test\ ^
    -xr!node_modules\less\benchmark\ ^
    -xr!node_modules\less\dist\
    
%zip% a ..\Resources\nodejs.tgz %temp%\nodejs.tar
popd