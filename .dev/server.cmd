@ECHO OFF
setlocal
where bundle.bat >nul
if errorlevel 1 ECHO Cannot find bundle
call %~dp0vars.cmd
where bundle.bat >nul
if errorlevel 1 GOTO :EOF
pushd %~dp0..
call bundle exec ruhoh server 9292
:: http://127.0.0.1:9292/no-new-legacy/
popd
