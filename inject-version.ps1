# inject-version.ps1
# Script to inject version information into PowerPlanTools module files

# Generate version in format yyyy.MM.dd.HHmm
$version = Get-Date -Format "yyyy.MM.dd.HHmm"
Write-Host "Injecting version: $version"

# Update VersionInfo.cs
$versionInfoPath = Join-Path $PSScriptRoot "src\VersionInfo.cs"
$versionInfoContent = @"
using System.Reflection;

// Version information for the assembly
// This file will be updated by inject-version.ps1 script
[assembly: AssemblyVersion("$version")]
[assembly: AssemblyFileVersion("$version")]
[assembly: AssemblyInformationalVersion("$version")]
"@
Set-Content -Path $versionInfoPath -Value $versionInfoContent
Write-Host "Updated $versionInfoPath"

# Update PowerPlanTools.psd1
$manifestPath = Join-Path $PSScriptRoot "PowerPlanTools.psd1"
$manifestContent = Get-Content -Path $manifestPath -Raw
$manifestContent = $manifestContent -replace '(ModuleVersion\s*=\s*[''"]).*?([''"])', "`$1$version`$2"
Set-Content -Path $manifestPath -Value $manifestContent
Write-Host "Updated $manifestPath"

# Create release directory
$releaseDir = Join-Path $PSScriptRoot "Releases\$version"
if (-not (Test-Path $releaseDir)) {
    New-Item -Path $releaseDir -ItemType Directory -Force | Out-Null
    Write-Host "Created release directory: $releaseDir"
}

Write-Host "Version injection complete: $version"
