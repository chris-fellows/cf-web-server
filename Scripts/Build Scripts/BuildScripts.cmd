@echo off
powershell -File "BuildScripts.ps1" -SourceFolder "%~dp0.." -DestinationFolder "D:\Data\Temp\EventHandlerData\Scripts"
echo Done
pause