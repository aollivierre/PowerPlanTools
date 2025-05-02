# build.ps1
# Script to build the PowerPlanTools module

# Update version information
Write-Host "Updating version information..."
.\inject-version.ps1

# Build the module
Write-Host "Building module..."
dotnet build src/PowerPlanTools.csproj -c Release

# Copy module files to release directory
$version = Get-Date -Format "yyyy.MM.dd.HHmm"
$releaseDir = Join-Path $PSScriptRoot "Releases\$version"
$moduleDir = Join-Path $PSScriptRoot "Module"

Write-Host "Copying module files to release directory: $releaseDir"

# Create release directory if it doesn't exist
if (-not (Test-Path $releaseDir)) {
    New-Item -Path $releaseDir -ItemType Directory -Force | Out-Null
}

# Copy module files
Copy-Item -Path "$moduleDir\*" -Destination $releaseDir -Recurse -Force
Copy-Item -Path "PowerPlanTools.psd1" -Destination $releaseDir -Force
Copy-Item -Path "LICENSE" -Destination $releaseDir -Force
Copy-Item -Path "README.md" -Destination $releaseDir -Force

# Copy types directory
Copy-Item -Path "types" -Destination $releaseDir -Recurse -Force

Write-Host "Build completed successfully. Module files are in: $releaseDir"
