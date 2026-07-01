@echo off
:: This script will be run after every build
set "config=%1"
set "base=..\..\bin\%config%\net9.0"
set "plugins=%base%\Plugins"
set "sources=..\Plugins"

:: Check if the directory exists
if not exist "%plugins%" (
	mkdir "%plugins%"
)

:: Build, then copy each of the plugins
FOR /D %%D IN ("%sources%\*") DO (
	echo Building plugin %%~nxD...

	:: Build the plugin's project. --no-restore skips the (often slow)
	:: restore step; MSBuild's own incremental engine still skips
	:: recompiling source files that haven't changed.
	for %%P in ("%%D\*.csproj") do (
		dotnet clean "%%P" -c %config%
		dotnet build "%%P" -c %config% --no-restore --nologo -v quiet --no-incremental
	)

	:: Ensure destination folder exists
	if not exist "%plugins%\%%~nxD" (
		mkdir "%plugins%\%%~nxD"
	)

	:: Copy all build output
	xcopy "%%D\bin\%config%\net9.0" "%plugins%\%%~nxD" /E /I /Y >nul

	:: Copy the plugin's main DLL renamed to anycpu.dll
	if exist "%%D\bin\%config%\net9.0\%%~nxD.dll" (
		copy /Y "%%D\bin\%config%\net9.0\%%~nxD.dll" "%plugins%\%%~nxD\anycpu.dll" >nul
	) else (
		echo WARNING: could not find %%~nxD.dll to copy as anycpu.dll
	)
)