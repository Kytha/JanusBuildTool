@echo off

rem // Copyright (c) Kyle Thatcher. All rights reserved.

setlocal
pushd
echo Generating Janus Engine project files...

call "../Binaries/JanusBuildTool.exe" -build -platforms=Windows,Linux,MacOS %*
if errorlevel 1 goto BuildToolFailed

popd
echo Finished!
pause
exit /B 0

:BuildToolFailed
echo JanusBuildTool tool failed.
pause
goto Exit

:Exit
popd
exit /B 1