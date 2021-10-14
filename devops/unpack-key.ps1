param (
    [Parameter(Mandatory = $true)][string]$keyFile
)

$jsonKey = Get-Content $keyFile | ConvertFrom-Json

$keyString = $jsonKey.KeyMaterial
$name = $jsonKey.KeyName

$key = $keyString.Replace("\n", "`n")

$key > "output/$($name).pem"