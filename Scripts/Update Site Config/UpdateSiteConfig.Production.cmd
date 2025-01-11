@echo off
powershell -File "GetHTTPResponse.ps1" -ConfigFile "Config.Production.json" -RequestFile "UpdateSiteConfig.Request.json"
echo Done
pause