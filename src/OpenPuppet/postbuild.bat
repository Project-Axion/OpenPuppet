::@echo off

:: This script will be run after every build

set "config=%1"
set "base=..\..\bin\%config%\net9.0"
set "plugins=%base%\Plugins"
set "sources=..\Plugins"

:: Check if the directory exists
if not exist "%plugins%" (
	mkdir "%plugins%"
)

:: Copy each of the plugins
FOR /D %%D IN ("%sources%\*") DO (
    xcopy "%%D\bin\%config%\net8.0" "%plugins%\%%~nxD" /E /I /Y >nul
)