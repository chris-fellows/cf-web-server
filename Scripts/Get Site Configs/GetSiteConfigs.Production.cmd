@echo off
powershell -File "GetHTTPResponse.ps1" -ConfigFile "Config.Production.json" -RequestFile "GetSiteConfigs.Request.json"
echo Done
pause