@echo off
pushd "%~dp0\nodejs"

if not exist node_modules\coffee-script (
    npm install coffee-script
)

if not exist node_modules\less (
    npm install less
)

set zip="%~dp0..\..\tools\7-zip\7z.exe"

if exist %temp%\nodejs.tar del %temp%\nodejs.tar
if exist ..\Resources\nodejs.tgz del ..\Resources\nodejs.tgz

%zip% a -r %temp%\nodejs.tar * ^
    -xr!node_modules\npm\ ^
    -xr!node_modules\less\test\ ^
    -xr!node_modules\less\benchmark\ ^
    -xr!node_modules\less\dist\
    
%zip% a ..\Resources\nodejs.tgz %temp%\nodejs.tar
popd