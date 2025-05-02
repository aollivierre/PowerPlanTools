# PowerPlanTools

PowerPlanTools is a PowerShell Binary Module for native management of Windows Power Plans and Power Settings. It provides a comprehensive set of cmdlets for managing power plans without relying on powercfg.exe.

## Features

* Native power plan and setting manipulation using WMI and PowrProf.dll (no powercfg.exe)
* Full alias support for known power setting GUIDs
* Optional raw GUID input for advanced use
* Tab-completion for PlanName, SettingAlias, and SettingGuid
* Export/Import of power plans using native API (JSON, CSV, or XML formats)
* Advanced search capabilities with regex and wildcard pattern support
* Power plan statistics over time using event logs
* Full metadata output (`-WithMetadata`)
* Support for hidden settings (`-IncludeHidden`)
* Pipeline support for all relevant cmdlets
* Full support for `-WhatIf`, `-Confirm`, `-Force`
* Detailed verbose logging with timestamps and progress counters

## Installation

### Option 1: Install from GitHub Release

1. Download the latest release ZIP file from the [Releases page](https://github.com/GraceSolutions/PowerPlanTools/releases)
2. Extract the ZIP file to a temporary location
3. Run the included `Install-PowerPlanTools.ps1` script, which will:
   - Find the appropriate PowerShell modules directory
   - Create a PowerPlanTools folder if it doesn't exist
   - Copy all module files to the correct location

### Option 2: Manual Installation

1. Download the latest release ZIP file from the [Releases page](https://github.com/GraceSolutions/PowerPlanTools/releases)
2. Extract the ZIP file to a temporary location
3. Copy the `PowerPlanTools` folder to one of these locations:
   - `$env:USERPROFILE\Documents\WindowsPowerShell\Modules\` (for current user)
   - `$env:ProgramFiles\WindowsPowerShell\Modules\` (for all users, requires admin)

### Option 3: Install from Source

```powershell
# Clone the repository
git clone https://github.com/GraceSolutions/PowerPlanTools.git
cd PowerPlanTools

# Build the module
.\build.ps1

# Install the module
.\Releases\<version>\Install-PowerPlanTools.ps1
```

## Quick Start

```powershell
# Import the module
Import-Module PowerPlanTools

# List all available power plans
Get-PowerPlan

# Get details about the current power plan
Get-PowerPlan -Current

# Find power settings related to display
Find-PowerSetting -SearchString "display"

# Find power settings using regex pattern
Find-PowerSetting -SearchString "^processor.*state$" -Regex

# Find power settings using wildcard pattern
Find-PowerSetting -SearchString "USB*suspend*" -Wildcard

# Export a power plan to JSON
Export-PowerSettings -PlanName "Balanced" -Path "C:\Temp\BalancedPlan.json"

# Import power settings from a file
Import-PowerSettings -Path "C:\Temp\BalancedPlan.json" -PlanName "Custom Plan" -CreateIfNotExists
```

## Available Cmdlets

### Power Plan Management
- `Get-PowerPlan` - Lists all power plans or gets a specific plan
- `New-PowerPlan` - Creates a new power plan
- `Remove-PowerPlan` - Deletes a power plan
- `Set-PowerPlan` - Sets the active power plan
- `Reset-PowerPlanDefaults` - Resets a power plan to default settings

### Power Setting Management
- `Get-PowerSetting` - Gets power settings for a plan
- `Update-PowerSetting` - Updates a power setting value
- `Find-PowerSetting` - Searches for power settings by name, description, or GUID
- `Export-PowerSettings` - Exports power settings to a file (JSON, CSV, XML)
- `Import-PowerSettings` - Imports power settings from a file

### Analysis and Comparison
- `Compare-PowerPlans` - Compares settings between power plans
- `Get-PowerPlanStatistic` - Gets power plan usage statistics

## Documentation

* [CHANGELOG.md](docs/CHANGELOG.md) - Version history and changes
* [INSTALL.md](docs/INSTALL.md) - Detailed installation instructions
* [USAGE.md](docs/USAGE.md) - Detailed usage instructions and examples
* [API-SPEC.md](docs/API-SPEC.md) - API specification
* [MODULE-STRUCTURE.md](docs/MODULE-STRUCTURE.md) - Module structure documentation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
