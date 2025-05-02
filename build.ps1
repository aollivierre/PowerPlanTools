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

Write-Host "Copying module files to release directory: $releaseDir"

# Create release directory if it doesn't exist
if (-not (Test-Path $releaseDir)) {
    New-Item -Path $releaseDir -ItemType Directory -Force | Out-Null
}

# Create proper module structure
$moduleName = "PowerPlanTools"
$moduleReleaseDir = Join-Path $releaseDir $moduleName

# Create module directory
if (-not (Test-Path $moduleReleaseDir)) {
    New-Item -Path $moduleReleaseDir -ItemType Directory -Force | Out-Null
}

# Create directory structure
$libDir = Join-Path $moduleReleaseDir "lib"
$typesDir = Join-Path $moduleReleaseDir "types"

if (-not (Test-Path $libDir)) {
    New-Item -Path $libDir -ItemType Directory -Force | Out-Null
}

if (-not (Test-Path $typesDir)) {
    New-Item -Path $typesDir -ItemType Directory -Force | Out-Null
}

# Copy module manifest and supporting files
Copy-Item -Path "PowerPlanTools.psd1" -Destination $moduleReleaseDir -Force
Copy-Item -Path "LICENSE" -Destination $moduleReleaseDir -Force
Copy-Item -Path "README.md" -Destination $moduleReleaseDir -Force

# Copy types files
Copy-Item -Path "types\*.ps1xml" -Destination $typesDir -Force

# Copy DLLs to the lib directory
if (Test-Path "Module\net472\PowerPlanTools.dll") {
    Copy-Item -Path "Module\net472\PowerPlanTools.dll" -Destination $libDir -Force
    Write-Host "Copied PowerPlanTools.dll to lib directory"
}

# Copy NuGet dependencies
$nugetPackagesDir = Join-Path $env:USERPROFILE ".nuget\packages"

# Copy Newtonsoft.Json
$newtonsoftDir = Join-Path $nugetPackagesDir "newtonsoft.json\13.0.3\lib\net45"
if (Test-Path $newtonsoftDir) {
    Copy-Item -Path "$newtonsoftDir\*.dll" -Destination $libDir -Force
    Write-Host "Copied Newtonsoft.Json dependency"
}

# Copy TaskScheduler
$taskSchedulerDir = Join-Path $nugetPackagesDir "microsoft.win32.taskscheduler\2.2.0.3\lib\net452"
if (Test-Path $taskSchedulerDir) {
    Copy-Item -Path "$taskSchedulerDir\*.dll" -Destination $libDir -Force
    Write-Host "Copied TaskScheduler dependency"
}

# Remove the net472 and net6.0 directories as they're not needed anymore
if (Test-Path "$moduleReleaseDir\net472") {
    Remove-Item -Path "$moduleReleaseDir\net472" -Recurse -Force
    Write-Host "Removed net472 directory"
}

if (Test-Path "$moduleReleaseDir\net6.0") {
    Remove-Item -Path "$moduleReleaseDir\net6.0" -Recurse -Force
    Write-Host "Removed net6.0 directory"
}

# Create root level files for the release
Copy-Item -Path "LICENSE" -Destination $releaseDir -Force
Copy-Item -Path "README.md" -Destination $releaseDir -Force

# Create an install script
$installScript = @"
# Install-PowerPlanTools.ps1
# Script to install the PowerPlanTools module

# Determine the module path
`$modulePath = `$env:PSModulePath -split ';' | Where-Object { `$_ -like "*`$env:USERPROFILE*" } | Select-Object -First 1
if (-not `$modulePath) {
    `$modulePath = Join-Path `$env:USERPROFILE "Documents\WindowsPowerShell\Modules"
}

# Create the module directory if it doesn't exist
`$moduleDir = Join-Path `$modulePath "PowerPlanTools"
if (-not (Test-Path `$moduleDir)) {
    New-Item -Path `$moduleDir -ItemType Directory -Force | Out-Null
    Write-Host "Created module directory: `$moduleDir"
}

# Copy module files
Copy-Item -Path ".\PowerPlanTools\*" -Destination `$moduleDir -Recurse -Force
Write-Host "Copied module files to: `$moduleDir"

Write-Host "PowerPlanTools module installed successfully!"
Write-Host "To use the module, open a new PowerShell window and run: Import-Module PowerPlanTools"
"@

Set-Content -Path (Join-Path $releaseDir "Install-PowerPlanTools.ps1") -Value $installScript

# Copy the module files to the Module folder
$moduleFolder = Join-Path $PSScriptRoot "Module\PowerPlanTools"
if (Test-Path $moduleFolder) {
    # Clear the Module folder
    Remove-Item -Path "$moduleFolder\*" -Recurse -Force

    # Copy the module files
    Copy-Item -Path "$moduleReleaseDir\*" -Destination $moduleFolder -Recurse -Force
    Write-Host "Copied module files to Module folder"
}

Write-Host "Build completed successfully. Module files are in: $releaseDir"
Write-Host "Latest module version is also available in: $moduleFolder"
