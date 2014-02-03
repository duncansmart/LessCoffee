@echo off

if "%1"=="CLEAN" (
	if exist "%~dp0\Resources\nodejs.tgz" del "%~dp0\Resources\nodejs.tgz"
	goto :EOF
)

:: create httpget.js for use with cscript.exe
set HTTPGET_JS="%TEMP%\httpget.js"
echo (function(b,d){var a=b.Arguments(0);var c=b.Arguments(1);var f=new d("MSXML2.XMLHTTP");f.open("GET",a,false);f.send();if(f.Status==200){var e=new d("ADODB.Stream");e.Open();e.Type=1;e.Write(f.ResponseBody);e.Position=0;e.SaveToFile(c);e.Close()}else{b.Echo("Error: HTTP "+f.status+" "+f.statusText)}})(WScript,ActiveXObject); > %HTTPGET_JS%

if not exist "%~dp0\nodejs" md "%~dp0\nodejs"

pushd "%~dp0\nodejs"
set zip="%~dp0..\..\tools\7-zip\7z.exe"

if not exist node.exe (
    echo Downloading node ...
    cscript //nologo %HTTPGET_JS% http://nodejs.org/dist/latest/node.exe node.exe
)

if not exist npm.cmd (
    echo Downloading npm ...
    if exist npm.zip del npm.zip
    cscript //nologo %HTTPGET_JS% http://nodejs.org/dist/npm/npm-1.3.26.zip npm.zip
    echo Extracting npm ...
    %zip% x npm.zip >nul
    del npm.zip
)

call npm update --quiet

if not exist ..\Resources\nodejs.tgz (
    if not exist ..\Resources md ..\Resources
    
    if exist %temp%\nodejs.tar del %temp%\nodejs.tar

    %zip% a -r %temp%\nodejs.tar * ^
        -x!node_modules\npm\ ^
        -x!node_modules\less\.grunt\ ^
        -x!node_modules\less\.idea\ ^
        -x!node_modules\less\benchmark\ ^
        -x!node_modules\less\build\ ^
        -x!node_modules\less\dist\ ^
        -x!node_modules\less\projectFilesBackup\ ^
        -x!node_modules\less\test\ ^
        -x!node_modules\less\tmp\ ^
        >nul
        
    %zip% a ..\Resources\nodejs.tgz %temp%\nodejs.tar >nul
	del %temp%\nodejs.tar
)

popd