# Usage Guide

This guide provides examples of how to use the PowerPlanTools module to manage Windows power plans and settings.

## Getting Started

First, import the module:

```powershell
Import-Module PowerPlanTools
```

## Basic Commands

### List All Power Plans

```powershell
Get-PowerPlan
```

### Get the Active Power Plan

```powershell
Get-PowerPlan -Active
```

### Set the Active Power Plan

```powershell
Set-PowerPlan -Name "Balanced"
# or
Set-PowerPlan -Guid "381b4222-f694-41f0-9685-ff5bb260df2e"
```

### Get Power Settings for a Plan

```powershell
Get-PowerSetting -Name "Balanced"
# or
Get-PowerPlan -Name "Balanced" | Get-PowerSetting
# or with settings included directly
Get-PowerPlan -Name "Balanced" -IncludeSettings
```

### Update a Power Setting

```powershell
Update-PowerSetting -Name "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -OnBattery 5
# or
Update-PowerSetting -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -SettingGuid "7516b95f-f776-4464-8c53-06167f40cc99" -PluggedIn 15 -OnBattery 5
```

## Advanced Usage

### Create a New Power Plan

```powershell
New-PowerPlan -SourcePlanName "Balanced" -NewPlanName "My Custom Plan"
```

### Delete a Power Plan

```powershell
Remove-PowerPlan -Name "My Custom Plan" -Force
```

### Export Power Settings

```powershell
# Export to JSON (default)
Export-PowerSettings -Name "Balanced" -Path "C:\Reports\BalancedPlan.json"

# Export to CSV
Export-PowerSettings -Name "Balanced" -Path "C:\Reports\BalancedPlan.csv" -Format Csv

# Export to XML
Export-PowerSettings -Name "Balanced" -Path "C:\Reports\BalancedPlan.xml" -Format Xml
```

### Import Power Settings

```powershell
# Import from a file
Import-PowerSettings -Path "C:\Reports\BalancedPlan.json" -Name "Balanced"

# Import and create a new plan if it doesn't exist
Import-PowerSettings -Path "C:\Reports\BalancedPlan.json" -Name "Custom Plan" -CreateIfNotExists
```

### Compare Power Plans

```powershell
Compare-PowerPlans -ReferencePlanName "Balanced" -DifferencePlanName "Power saver"
```

### Reset Power Plan to Defaults

```powershell
Reset-PowerPlanDefaults -Name "Balanced" -Confirm:$false
```

### Get Power Plan Usage Statistics

```powershell
Get-PowerPlanStatsReport -Days 7
```

## Finding Power Settings and Subgroups

### Search for Power Settings

```powershell
# Search by name
Find-PowerSetting -SearchString "display"

# Search using regex pattern
Find-PowerSetting -SearchString "^processor.*state$" -Regex

# Search using wildcard pattern
Find-PowerSetting -SearchString "USB*suspend*" -Wildcard

# Search in a specific plan
Find-PowerSetting -Name "Balanced" -SearchString "processor"
```

### Search for Subgroups

```powershell
# List all subgroups
Find-SubGroup -Name "*"

# Find subgroups by name pattern
Find-SubGroup -Name "*processor*"

# Find subgroup by GUID
Find-SubGroup -SubGroupGuid "54533251-82be-4824-96c1-47b60b740d00"

# Find settings in a specific subgroup
Find-PowerSetting -SearchString "processor" | Where-Object { $_.SubGroupAlias -eq "Processor Power Management" }
```

## Power State Management

### Get Power State Settings

```powershell
# Get all power state settings
Get-PowerState

# Get specific power state settings
Get-PowerState -Property "HibernateEnabled"
```

### Configure Power State Settings

```powershell
# Enable hibernation
Set-PowerState -EnableHibernate $true

# Disable connected standby
Set-PowerState -EnableConnectedStandby $false

# Configure multiple settings at once
Set-PowerState -EnableHibernate $true -EnableFastStartup $false -EnableS3 $true
```

## Working with Hidden Settings

To include hidden settings in the output:

```powershell
Get-PowerSetting -Name "Balanced" -IncludeHidden
```

## Using Raw GUIDs

For advanced scenarios, you can use GUIDs directly:

```powershell
Update-PowerSetting -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -SubGroupGuid "0012ee47-9041-4b5d-9b77-535fba8b1442" -SettingGuid "7516b95f-f776-4464-8c53-06167f40cc99" -PluggedIn 15
```

## Pipeline Support

Most cmdlets support pipeline input:

```powershell
# Get settings and update them
Get-PowerPlan -Name "Balanced" | Get-PowerSetting | Where-Object { $_.Alias -like "*display*" } | Update-PowerSetting -PluggedIn 10

# Find settings in a specific subgroup and update them
Find-SubGroup -Name "*processor*" | ForEach-Object {
    Find-PowerSetting -SearchString "state" | Where-Object { $_.SubGroupGuid -eq $_.SubGroupGuid } | Update-PowerSetting -Name "Balanced" -PluggedIn 100
}

# Export settings from multiple plans
Get-PowerPlan | Where-Object { $_.Name -ne "Power saver" } | ForEach-Object {
    Export-PowerSettings -Name $_.Name -Path "C:\Reports\$($_.Name).json"
}
```

## WhatIf and Confirm Support

All cmdlets that make changes support -WhatIf and -Confirm:

```powershell
# See what would happen without making changes
Remove-PowerPlan -Name "My Custom Plan" -WhatIf
Update-PowerSetting -Name "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -WhatIf
Set-PowerState -EnableHibernate $true -WhatIf

# Confirm before making changes
Update-PowerSetting -Name "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -Confirm
Set-PowerState -EnableHibernate $true -Confirm

# Force changes without confirmation
Remove-PowerPlan -Name "My Custom Plan" -Force
```
