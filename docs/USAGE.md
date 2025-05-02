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
Get-PowerSetting -PlanName "Balanced"
# or
Get-PowerPlan -Name "Balanced" | Get-PowerSetting
```

### Update a Power Setting

```powershell
Update-PowerSetting -PlanName "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -OnBattery 5
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

### Export Power Settings Report

```powershell
Export-PowerSettingsReport -PlanName "Balanced" -Path "C:\Reports\BalancedPlan.json" -Format Json
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

## Working with Hidden Settings

To include hidden settings in the output:

```powershell
Get-PowerSetting -PlanName "Balanced" -IncludeHidden
```

## Using Raw GUIDs

For advanced scenarios, you can use GUIDs directly:

```powershell
Update-PowerSetting -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -SubGroupGuid "0012ee47-9041-4b5d-9b77-535fba8b1442" -SettingGuid "7516b95f-f776-4464-8c53-06167f40cc99" -PluggedIn 15
```

## Pipeline Support

Most cmdlets support pipeline input:

```powershell
Get-PowerPlan -Name "Balanced" | Get-PowerSetting | Where-Object { $_.Alias -like "*display*" } | Update-PowerSetting -PluggedIn 10
```

## WhatIf and Confirm Support

All cmdlets that make changes support -WhatIf and -Confirm:

```powershell
Remove-PowerPlan -Name "My Custom Plan" -WhatIf
Update-PowerSetting -PlanName "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -Confirm
```
