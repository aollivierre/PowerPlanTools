# Installation Instructions

## Prerequisites

- Windows 10 or later
- PowerShell 5.1 or PowerShell Core 6.0+
- .NET Framework 4.7.2 or .NET 6.0+

## Installation Methods

### Method 1: Install from PowerShell Gallery (Recommended)

```powershell
Install-Module -Name PowerPlanTools -Scope CurrentUser
```

### Method 2: Manual Installation

1. Download the latest release from the [GitHub Releases page](https://github.com/your-org/PowerPlanTools/releases)
2. Extract the ZIP file to a folder
3. Copy the extracted folder to one of your PowerShell module directories:

```powershell
# Find your PSModulePath
$env:PSModulePath -split ';'

# Copy the module to your preferred location, for example:
Copy-Item -Path ".\PowerPlanTools" -Destination "$env:USERPROFILE\Documents\PowerShell\Modules\" -Recurse
```

## Verification

To verify that the module is installed correctly, run:

```powershell
Import-Module PowerPlanTools
Get-Module PowerPlanTools
```

You should see the module information displayed, including the version number.

## Updating

To update the module from PowerShell Gallery:

```powershell
Update-Module -Name PowerPlanTools
```

## Uninstallation

To uninstall the module:

```powershell
Uninstall-Module -Name PowerPlanTools
```
