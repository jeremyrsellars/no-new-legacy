where bundle
if not errorlevel 1 goto :EOF

:: Put ruby and its development kit in the system path
call C:\Ruby25-x64\bin\setrbvars.bat
::call "C:\Users\jeremy.sellars\Downloads\RubyDevKit\devkitvars.bat"
