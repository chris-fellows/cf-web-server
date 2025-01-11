@echo off
powershell -File "GetHTTPResponse.ps1" -ConfigFile "Config.Development.json" -RequestFile "GetSiteConfigs.Request.json"
echo Done
pause