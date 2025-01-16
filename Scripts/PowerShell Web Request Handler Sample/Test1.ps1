param($Method, $URL, $ContentBase64, $Parameters)

function GetResponseContent($MethodParam, $URLParam, $ContentParam, $ParametersParam) {

    $Param1Value = $ParametersParam["Param1"]
    $Param2Value = $ParametersParam["Param2"]

    $Response= "<html>" +
        "<head>" +
            "<script>" +
            "</script> " +
        "</head>" +
        "<body"> +
        "Method: $MethodParam<BR/>" +
        "URL: $URLParam<BR/>" +        
        "Param1: $Param1Value<BR/>" +
        "Param2: $Param2Value<BR/>" +
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

# Get response content
$ResponseContent = GetResponseContent -MethodParam $Method -URLParam $URL -ContentParam $Content -ParametersParam $Parameters

$StatusCode = 200

Write-Host "Parameters $($Parameters.ToString())"

#Write-Host "Response content to return is $ResponseContent"

Write-Host "Status-Code=$StatusCode"

# Return content type
Write-Host "Content-Type=text/html"

# Return content
$ContentBase64 = ConvertToBase64String -StringValue $ResponseContent
Write-Host "Content-Base64=$ContentBase64"