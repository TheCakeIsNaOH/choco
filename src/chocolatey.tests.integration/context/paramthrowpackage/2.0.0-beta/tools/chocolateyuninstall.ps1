$pp = Get-PackageParameters
if ($pp['UpgradePackageOnlyParameter']) {
    Throw "Error: Found the 'UpgradePackageOnlyParameter'"
}

Write-Output "$env:PackageName $env:PackageVersion Uninstalled"