Import-Module WebAdministration

Set-ItemProperty "IIS:\AppPools\DefaultAppPool" -Name "processModel.loadUserProfile" -Value "True"