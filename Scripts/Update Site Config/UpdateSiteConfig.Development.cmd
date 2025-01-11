@echo off
powershell -File "GetHTTPResponse.ps1" -ConfigFile "Config.Development.json" -RequestFile "UpdateSiteConfig.Request.json"
echo Done
pause