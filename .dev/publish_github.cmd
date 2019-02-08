@ECHO OFF
setlocal
call "%~dp0vars.cmd"
echo.
echo ===========================================================
echo Please be sure that all directories and files are unlocked before continuing.
echo This is running in %CD%
pause
bundle exec ruhoh publish github
