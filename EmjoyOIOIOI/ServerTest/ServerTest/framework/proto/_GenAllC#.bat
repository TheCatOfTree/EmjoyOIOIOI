@echo off

set "PROTOC_EXE=%cd%\Tools\protoc.exe"
set "WORK_DIR=%cd%\Proto"
set "CS_OUT_PATH=%cd%\CS"
::if not exist %CS_OUT_PATH% md %CS_OUT_PATH%

for /f "delims=" %%i in ('dir /b Proto "Proto/*.proto"') do (
   echo gen proto/%%i...
   "%PROTOC_EXE%"  --proto_path="%WORK_DIR%" --csharp_out="%CS_OUT_PATH%" "%WORK_DIR%\%%i"
   )
echo finish... 
