# ****************************************************************************
# **** This is script is embedded in the build server, use it to test only
# ****************************************************************************
$versionUrl = "https://www.google.com/".TrimEnd("/")
$versionUrl = $versionUrl + "/asd22"
function TestUrl([string]$url) {
    $HTTP_Response = $null
    try {
        $HTTP_Request = [System.Net.WebRequest]::Create($url)
        $HTTP_Response = $HTTP_Request.GetResponse()
        $HTTP_Status = [int]$HTTP_Response.StatusCode
        If ($HTTP_Status -eq 200) {
            return $true
        }
    }
    catch {        
    }
    finally {
        if ($HTTP_Response) {
            $HTTP_Response.Close()
        }        
    }
    return $false    
}
function TestUrlDelay([string]$url, [int]$times, [int]$delay) {
    [int]$count = 0;
    while ($count -lt $times) {
        Write-Host $delay second delay
        Start-Sleep $delay        
        Write-Host Pinging $url
        if (TestUrl $url) {
            return $true;    
        }
        $count = $count + 1
    }
    return $false;
}
$online = TestUrlDelay $versionUrl 4 2
if ($online) {
    Write-Host $versionUrl found
    Exit 0
}
else {
    Write-Host $versionUrl not found
    Exit 404
}