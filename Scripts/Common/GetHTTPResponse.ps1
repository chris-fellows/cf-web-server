param($ConfigFile,
      $RequestFile)

function GetConfig($File) {        
    $Config = (Get-Content -Path $File | ConvertFrom-Json)
    return $Config
}

function GetRequest($File) {
    $Request = (Get-Content -Path $File | ConvertFrom-Json)
    return $Request
}

function GetParameterValue($Parameter) {
    
    $ParameterValue = ""
    if ($Parameter.Type.ToLower() -eq "prompt") {    # Prompt for value
        $ParameterValue = Read-Host $Parameter.Prompt                
    }
    elseif ($Parameter.Type.ToLower() -eq "literal") {   # Literal value
        $ParameterValue = $Parameter.Value
    }       
    elseif ($Parameter.Type.ToLower() -eq "currentfile") {
        # Prompt until valid file
        $GotFile = $false
        while ($GotFile -eq $false)
        {
            $ParameterValue = Read-Host $Parameter.Prompt
              
            if ($ParameterValue -eq $null -or $ParameterValue -eq "") {
                Write-Host "File is not valid"
            }       
            elseif (!(Test-Path -Path $ParameterValue)) {
                Write-Host "File does not exist"
            } else {
                $GotFile = $true
            }
        }
    }

    return $ParameterValue
}

function GetResponseForRequest($Config, $Request) {
           
    # Set URL with placeholders replaced
    $URL = $Request.URL.Replace("#Config.APIURL#", $Config.APIURL)
   
    # Get input parameters. Either literal value or prompt user for value
    $Parameters = New-Object System.Collections.Generic.Dictionary"[String,String]"
    if ($Request.Parameters -ne $null) {
        foreach($Parameter in $Request.Parameters) {                        
            # Get parameter value            
            $ParameterValue = GetParameterValue -Parameter $Parameter

            # Encode            
            if ($Parameter.URLEncode -eq $true) {
                    $ParameterValue = [System.Web.HttpUtility]::UrlEncode($ParameterValue)
            }
            
            $Item1 = $Parameters.Add($Parameter.Name, $ParameterValue)
        }
    }

    # Replace parameters in URL
    foreach($ParameterKey in $Parameters.Keys) {              
        $URL = $URL.Replace("#Parameters.$ParameterKey#", $Parameters[$ParameterKey])        
    }

    # Set headers, replace placeholders
    $Headers = New-Object System.Collections.Generic.Dictionary"[String,String]"
    foreach($Header in $Request.Headers) {        
        # Set header value with placeholders replaced
        $HeaderValue = $Header.Value        

        # Replace parameters
        foreach($ParameterKey in $Parameters.Keys) {                                                
            $HeaderValue = $HeaderValue.Replace("#Parameters.$($ParameterKey)#", $Parameters[$ParameterKey])
        }

        #Write-Host "Adding header $($Header.Name) with $HeaderValue"        
        $Item = $Headers.Add($Header.Name, $HeaderValue)
    }

    # Get content
    $Content = $null
    if ($Request.ContentFile -ne $null -and $Request.ContentFile -ne $null -and $Request.ContentFile -ne "") {

        $ContentFile = $Request.ContentFile
        
        # Replace parameters
        foreach($ParameterKey in $Parameters.Keys) {                                                
            $ContentFile = $ContentFile.Replace("#Parameters.$($ParameterKey)#", $Parameters[$ParameterKey])
        }

        $Content = Get-Content -Path $ContentFile -Encoding UTF8        
    }

    $OutputFile = $Request.Response.ContentFile
    if ($OutputFile -eq $null -or $OutputFile -eq "") {
        $OutputFile = $null
    }

    #throw "Test error"
   
    $params = @{    
        Uri         = $URL        
        Method      = $Request.Method
        Body        = $Content        
        UserAgent   = $Request.UserAgent
        TimeoutSec    = 60
    }

    # Delete existing output file    
    if ($OutputFile -ne $null) {
        if (Test-Path -Path $OutputFile) {
            Remove-Item -Path $OutputFile -Force
        }
    }
   
    # Get results
    Write-Host "Getting response"
    $ReturnValue = 1
    try
    {        
        # Execute request and save response                
        if ($OutputFile -ne $null) {            
            if ($OutputFile.ToLower().EndsWith(".json")) {                
                $Result = Invoke-RestMethod @params -Headers $Headers                
                Set-Content -Path $OutputFile -Value (ConvertTo-Json $Result -Depth 10)
            }
            else {   # Non-JSON response                
                $Result = Invoke-RestMethod @params -Headers $Headers -OutFile $OutputFile                
            }

            Write-Host "Response saved to $OutputFile"
        } else {
            $Result = Invoke-RestMethod @params -Headers $Headers        
            Write-Host "Not saving response"
        }           

        $ReturnValue = 0
    }
    catch
    {
        $StatusCode = [int]$_.Exception.Response.StatusCode
        
        # Get response
        $Result = $_.Exception.Response.GetResponseStream()
        $Reader = New-Object System.IO.StreamReader($Result)
        $Reader.BaseStream.Position = 0
        $Reader.DiscardBufferedData()
        $ResponseBody = $Reader.ReadToEnd();

        Set-Content -Pass $OutputFile -Value $ResponseBody

        Write-Host "Error: Status returned is $StatusCode. Output saved to $OutputFile"

        $ReturnValue = 1
    }  
            
    return $ReturnValue
}

# Get config
$Config = GetConfig -File $ConfigFile

# Get request
$ResultFileCurrent = $RequestFile
if ($ResultFileCurrent -eq $null -or $ResultFileCurrent -eq "") {
    $ResultFileCurrent = Read-Host "Enter file containing request details"
}
if (!(Test-Path -Path $ResultFileCurrent)) {
    throw "File $ResultFileCurrent does not exist"
}

$Request = GetRequest -File $ResultFileCurrent

# Get response
$Result = GetResponseForRequest -Config $Config -Request $Request


