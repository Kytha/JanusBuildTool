@echo off

rem // Copyright (c) Kyle Thatcher. All rights reserved.

if not exist "Scripts\Windows\GetMSBuildPath.bat" goto Error_InvalidLocation

call "Scripts\Windows\GetMSBuildPath.bat"
if errorlevel 1 goto Error_NoVisualStudioEnvironment

if not exist "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" goto Compile
for /f "delims=" %%i in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath') do (
	for %%j in (15.0, Current) do (
		if exist "%%i\MSBuild\%%j\Bin\MSBuild.exe" (
			set MSBUILD_PATH="%%i\MSBuild\%%j\Bin\MSBuild.exe"
			goto Compile
		)
	)
)

:Compile
md Cache\Intermediate >nul 2>nul
dir /s /b Source\*.cs >Cache\Intermediate\JanusBuildTool.Files.txt
fc /b Cache\Intermediate\Build\JanusBuildTool.Files.txt Cache\Intermediate\Build\JanusBuildTool.PrevFiles.txt >nul 2>nul
if not errorlevel 1 goto SkipClean
echo %MSBUILD_PATH%
copy /y Cache\Intermediate\Build\JanusBuildTool.Files.txt Cache\Intermediate\Build\JanusBuildTool.PrevFiles.txt >nul
%MSBUILD_PATH% /nologo /verbosity:quiet Source\JanusBuildTool.csproj /property:Configuration=Release /property:Platform=AnyCPU /target:Clean

:SkipClean
%MSBUILD_PATH% /nologo /verbosity:quiet Source\JanusBuildTool.csproj /property:Configuration=Release /property:Platform=AnyCPU /target:Build

if errorlevel 1 goto Error_CompilationFailed

Binaries\JanusBuildTool.exe %*

if errorlevel 1 goto Error_JanusBuildToolFailed
exit /B 0


:Error_InvalidLocation
echo CallBuildTool ERROR: The script is in invalid directory.
goto Exit
:Error_NoVisualStudioEnvironment
echo CallBuildTool ERROR: Missing Visual Studio 2015 or newer.
goto Exit
:Error_CompilationFailed
echo CallBuildTool ERROR: Failed to compile JanusBuildTool project.
goto Exit
:Error_JanusBuildToolFailed
echo CallBuildTool ERROR: JanusBuildTool tool failed.
goto Exit
:Exit
exit /B 1