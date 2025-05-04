# Publish-PowerPlanTools.ps1
# This script publishes the PowerPlanTools module to the PowerShell Gallery

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter()]
    [switch]$WhatIf
)

# Get the module path
$modulePath = Join-Path $PSScriptRoot "Module\PowerPlanTools"

# Check if the module exists
if (-not (Test-Path $modulePath)) {
    Write-Error "Could not find the PowerPlanTools module at $modulePath"
    return
}

# Get the module version
$manifestPath = Join-Path $modulePath "PowerPlanTools.psd1"
$manifest = Import-PowerShellDataFile -Path $manifestPath
$version = $manifest.ModuleVersion

Write-Host "Publishing PowerPlanTools version $version to PowerShell Gallery..." -ForegroundColor Yellow

# Publish the module
try {
    $publishParams = @{
        Path = $modulePath
        NuGetApiKey = $ApiKey
        Repository = "PSGallery"
        ProjectUri = "https://github.com/Grace-Solutions/PowerPlanTools"
        Tags = @("Power", "Battery", "Settings", "Windows")
        ReleaseNotes = $manifest.PrivateData.PSData.ReleaseNotes
    }
    
    if ($WhatIf) {
        $publishParams.Add("WhatIf", $true)
    }
    
    Publish-Module @publishParams
    
    if (-not $WhatIf) {
        Write-Host "PowerPlanTools version $version published successfully to PowerShell Gallery!" -ForegroundColor Green
    } else {
        Write-Host "WhatIf: PowerPlanTools version $version would be published to PowerShell Gallery" -ForegroundColor Cyan
    }
} catch {
    Write-Error "Failed to publish PowerPlanTools to PowerShell Gallery: $_"
}
