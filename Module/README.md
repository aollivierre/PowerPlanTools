# PowerPlanTools Module Folder

This folder contains the latest version of the PowerPlanTools module in a properly organized structure.

## Purpose

The Module folder is maintained to provide easy access to the latest version of the PowerPlanTools module without having to navigate through version-specific folders in the Releases directory.

## Structure

```
Module/
└── PowerPlanTools/
    ├── lib/
    │   ├── Newtonsoft.Json.dll
    │   ├── PowerPlanTools.dll
    │   └── System.Management.Automation.dll
    ├── types/
    │   ├── PowerPlanTools.Format.ps1xml
    │   └── PowerPlanTools.Types.ps1xml
    ├── LICENSE
    ├── PowerPlanTools.psd1
    └── README.md
```

## Usage

This folder is automatically updated by the build script whenever a new version of the module is built. The contents of this folder can be copied directly to a PowerShell module path for installation.

## Installation

To install the module from this folder:

```powershell
# Copy the module to the user's module path
$modulePath = "$env:USERPROFILE\Documents\WindowsPowerShell\Modules"
Copy-Item -Path ".\Module\PowerPlanTools" -Destination $modulePath -Recurse -Force

# Import the module
Import-Module PowerPlanTools
```
