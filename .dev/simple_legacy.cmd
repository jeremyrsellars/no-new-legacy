@ECHO OFF
setlocal
where bundle.bat >nul
if errorlevel 1 ECHO Cannot find bundle
call vars.cmd
where bundle.bat >nul
if errorlevel 1 GOTO :EOF
pushd ..
set compiled_path=..\legacy_staging\blog
if not exist %compiled_path% md %compiled_path%
call bundle exec ruhoh compile %compiled_path%
pushd %compiled_path%
ECHO ===============================
dir /s /b /a-d|%systemroot%\System32\find.exe /i "faq"
ECHO ===============================
call static  "" --port 4000 
popd
popd
