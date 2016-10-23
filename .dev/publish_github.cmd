@ECHO OFF
setlocal
call "%~dp0vars.cmd"
bundle exec ruhoh publish github
