param($Method,
     $URL, 
     $ContentBase64, 
     $Form,       # Dictionary for form values
     $Parameters, # Dictionary for querystring parameters
     $Headers,    # Dictionary for request headers
     $SiteParameters  # Dictionary for site parameters. E.g. Connection string
)

function GetDictionaryForDisplay($InputParam) {
     $Values = "<table><tr><th>Name</th><th>Value</th></tr>"

     $ValueCount = 0
     foreach($Key in $InputParam.Keys) {
        
        $Value = $InputParam[$Key]
        
        $Values = $Values + "<tr><td>" + $Key + "</td><td>" + $Value + "</td></tr>"
        $ValueCount += 1
    }     

    if ($ValueCount -eq 0) {
        $Values = $Values + "<tr><td>None</td><td>None</td></tr>"
    }

    $Values = $Values + "</table>"

    return $Values
}

function GetResponseContent($MethodParam, $URLParam, $ContentParam, $FormParam, $ParametersParam, $HeadersParam, $SiteParametersParam) {

    # Get header values
    $HeaderValues = GetDictionaryForDisplay -InputParam $HeadersParam   

    # Get param values
    $ParamValues = GetDictionaryForDisplay -InputParam $ParametersParam        

    # Get form values
    $FormValues = GetDictionaryForDisplay -InputParam $FormParam        

    # Get site parameter values
    $SiteParameterValues = GetDictionaryForDisplay -InputParam $SiteParametersParam        

    $Response= "<html>" +
        "<head>" +
            "<script>" +
            "</script> " +
            "<style>" + 
            "table, th, td { " +
                "border: 1px solid black; " +
                "border-collapse: collapse; " +
            "} " +
            "</style>" +
        "</head>" +
        "<body>" +
        "<a href='NameForm.html'>Back</a><BR/><BR/>" +
        "<B>Method:</B> $MethodParam<BR/>" +
        "<B>URL:</B>$URLParam<BR/><BR/>" +            
        "<B>Headers</B><BR/>" +
        $HeaderValues + "<BR/>" +    
        "<B>Parameters</B><BR/>" +
        $ParamValues + "<BR/>" +
        "<B>Form</B><BR/>" +
        $FormValues + "<BR/>" + 
        "<B>Site Parameters</B><BR/>" +
        $SiteParameterValues + "<BR/>" + 
        "</body>" +
        "</html>"

    return $Response
}

function ConvertToBase64String($StringValue) {

    $Bytes = [System.Text.Encoding]::UTF8.GetBytes($StringValue)    
    $Result = [System.Convert]::ToBase64String($Bytes)
    return $Result
}

Write-Host "RequestContent=$ContentBase64"

# Get content from content base 64 string
$Content = [System.Convert]::FromBase64String($ContentBase64) 

# Get response conten t
$ResponseContent = GetResponseContent -MethodParam $Method -URLParam $URL -ContentParam $Content -FormParam $Form -ParametersParam $Parameters -HeadersParam $Headers -SiteParametersParam $SiteParameters
$ResponseContentType = "text/html"

$StatusCode = 200

Write-Host "Parameters $($Parameters.ToString())"

Write-Host "Status-Code=$StatusCode"

# Return content type
Write-Host "Content-Type=$ResponseContentType"

# Return content
$ContentBase64 = ConvertToBase64String -StringValue $ResponseContent
Write-Host "Content-Base64=$ContentBase64"