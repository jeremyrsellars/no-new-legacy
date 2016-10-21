@ECHO OFF
SETLOCAL

echo %cd% |find " " >nul
IF ERRORLEVEL 1 ECHO Sorray, This script doesn't work when there are spaces in the directory.
IF ERRORLEVEL 1 GOTO :EOF

call "%~dp0vars.cmd"
set thecmd=%~dp0simple_legacy.cmd
::   set thecmd=comp
set expected_window_title=%systemroot%\system32\cmd.exe - %thecmd%
::   set expected_window_title=%systemroot%\system32\comp.exe
echo Looking for running server with title:%expected_window_title%
taskkill /f /T /fi "WINDOWTITLE eq %expected_window_title%"
timeout /t 3
start %thecmd%
