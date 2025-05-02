# Module Structure

This document describes the structure of the PowerPlanTools module.

## Directory Structure

```
PowerPlanTools/
├── src/                    # C# source code only
│   ├── Cmdlets/            # PowerShell cmdlet implementations
│   ├── Models/             # Data models
│   ├── Utils/              # Utility classes
│   └── VersionInfo.cs      # Version information
│
├── lib/                    # External DLL dependencies
│   ├── Newtonsoft.Json.dll
│   ├── Microsoft.Win32.TaskScheduler.dll
│
├── types/                  # PowerShell XML type/format definitions
│   ├── PowerPlanTools.Types.ps1xml
│   └── PowerPlanTools.Format.ps1xml
│
├── Module/                 # Latest version module
│   ├── PowerPlanTools.psd1
│   ├── lib/
│   ├── types/
│   └── PowerPlanTools.dll
│
├── Releases/yyyy.MM.dd.HHmm/   # Versioned builds
│   ├── PowerPlanTools.psd1
│   ├── lib/
│   ├── types/
│   └── README.md
│
├── Artifacts/              # Logs, temp files, zip output
│   ├── build.log
│   └── *.zip
│
├── docs/                   # Documentation
│   ├── CHANGELOG.md
│   ├── INSTALL.md
│   ├── USAGE.md
│   ├── API-SPEC.md
│   └── MODULE-STRUCTURE.md
│
├── README.md               # High-level summary
├── LICENSE                 # Open-source license
├── inject-version.ps1      # Version stamping script
└── .gitignore              # Git ignore file
```

## Key Components

### Models

The module defines three main data models:

1. **PowerPlan** - Represents a Windows power plan
2. **PowerSetting** - Represents a power setting within a power plan
3. **PowerPlanStats** - Represents statistics for a power plan

### Utilities

The module includes several utility classes:

1. **WmiHelper** - Helper for WMI operations related to power plans
2. **PowerProfileHelper** - Helper for PowrProf.dll interop operations
3. **EventLogHelper** - Helper for event log operations related to power plans

### Cmdlets

The module provides 10 cmdlets:

1. **Get-PowerPlan** - List power plans
2. **Set-PowerPlan** - Set active power plan
3. **New-PowerPlan** - Clone a plan
4. **Remove-PowerPlan** - Delete a plan
5. **Update-PowerSetting** - Update power setting values
6. **Get-PowerSetting** - List all settings in a plan
7. **Export-PowerSettingsReport** - Export plan settings to file
8. **Compare-PowerPlans** - Show differences between two plans
9. **Reset-PowerPlanDefaults** - Restore default plan settings
10. **Get-PowerPlanStatsReport** - Generate usage/duration reports

### Type and Format Definitions

The module includes XML files for PowerShell type and format definitions:

1. **PowerPlanTools.Types.ps1xml** - Defines type extensions for the module's objects
2. **PowerPlanTools.Format.ps1xml** - Defines how the module's objects are displayed in the console

## Build Process

The module uses a versioning format of `yyyy.MM.dd.HHmm` (e.g., `2025.05.01.1630`).

The `inject-version.ps1` script:
1. Generates a version number based on the current date and time
2. Updates the version in `VersionInfo.cs` and `PowerPlanTools.psd1`
3. Creates a release directory with the version number

## Dependencies

The module depends on:
1. .NET Framework 4.7.2 or .NET 6
2. Newtonsoft.Json.dll
3. Microsoft.Win32.TaskScheduler.dll
