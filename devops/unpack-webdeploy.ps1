# ****************************************************************************
# **** This is script is embedded in webformation.json, use it to test only
# ****************************************************************************
$sourcefolder = "C:\temp\sitepackage"
$target = "C:\inetpub\site"
function RecreateDir($dir) {
    if ( Test-Path $dir ) { 
        Get-ChildItem -Path  $dir -Force -Recurse | Remove-Item -Force -Recurse
    }
    else {
        New-Item -ItemType Directory -Force -Path $dir    
    }
}

# msdeploy creates a web artifact with multiple levels of folders. We only need the content 
# of the folder that has Web.config within it 
function GetWebArtifactFolderPath($path) {
    foreach ($item in Get-ChildItem $path) {   
        if (Test-Path $item.FullName -PathType Container) {   
            # return the full path for the folder which contains Web.config
            if (Test-Path ($item.fullname + "\Web.config")) {
                return $item.FullName;
            }
            $found = GetWebArtifactFolderPath $item.FullName
            if ($found) {
                return $found
            }
        }
    }
    return $null
}
$sourcepath = GetWebArtifactFolderPath($sourcefolder)
if ($sourcepath) {
    $sourcefiles = $sourcepath + "\*"
    Write-Host "Deploying from " $sourcefiles

    # Clean up target directory
    RecreateDir($target)
    $targetFolder = $target + "\"
    Copy-Item $sourcefiles -Destination $targetFolder -Recurse -Force
}