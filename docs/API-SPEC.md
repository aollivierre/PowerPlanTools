# API Specification

This document provides detailed information about the PowerPlanTools module's cmdlets, parameters, and object models.

## Cmdlets

### Get-PowerPlan

Gets information about Windows power plans.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Name | String | The name of the power plan to retrieve |
| Guid | Guid | The GUID of the power plan to retrieve |
| Active | Switch | Whether to retrieve only the active power plan |
| IncludeSettings | Switch | Whether to include power settings in the output |
| IncludeHidden | Switch | Whether to include hidden power settings in the output |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

Returns one or more PowerPlan objects.

### Set-PowerPlan

Sets the active power plan.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Name | String | The name of the power plan to activate |
| Guid | Guid | The GUID of the power plan to activate |
| PassThru | Switch | Whether to return the activated power plan |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

If -PassThru is specified, returns the activated PowerPlan object.

### New-PowerPlan

Creates a new power plan by cloning an existing one.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| SourcePlanName | String | The name of the source power plan to clone |
| SourcePlanGuid | Guid | The GUID of the source power plan to clone |
| NewPlanName | String | The name for the new power plan |
| PassThru | Switch | Whether to return the new power plan |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

If -PassThru is specified, returns the new PowerPlan object.

### Remove-PowerPlan

Deletes a power plan.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Name | String | The name of the power plan to delete |
| Guid | Guid | The GUID of the power plan to delete |
| Force | Switch | Whether to suppress confirmation prompts |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

None.

### Get-PowerSetting

Gets power settings for a power plan.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| PlanName | String | The name of the power plan |
| PlanGuid | Guid | The GUID of the power plan |
| SettingAlias | String | The alias of the setting to retrieve |
| SettingGuid | Guid | The GUID of the setting to retrieve |
| SubGroupGuid | Guid | The GUID of the subgroup containing the setting |
| IncludeHidden | Switch | Whether to include hidden settings in the output |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

Returns one or more PowerSetting objects.

### Update-PowerSetting

Updates a power setting value.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| PlanName | String | The name of the power plan |
| PlanGuid | Guid | The GUID of the power plan |
| SettingAlias | String | The alias of the setting to update |
| SettingGuid | Guid | The GUID of the setting to update |
| SubGroupGuid | Guid | The GUID of the subgroup containing the setting |
| PluggedIn | Object | The value to set when plugged in |
| OnBattery | Object | The value to set when on battery |
| PassThru | Switch | Whether to return the updated setting |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

If -PassThru is specified, returns the updated PowerSetting object.

### Export-PowerSettingsReport

Exports power settings to a file.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| PlanName | String | The name of the power plan |
| PlanGuid | Guid | The GUID of the power plan |
| Path | String | The path to the output file |
| Format | String | The output format (Json, Csv, Xml) |
| IncludeHidden | Switch | Whether to include hidden settings in the output |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

None.

### Compare-PowerPlans

Compares two power plans and shows the differences.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| ReferencePlanName | String | The name of the reference power plan |
| ReferencePlanGuid | Guid | The GUID of the reference power plan |
| DifferencePlanName | String | The name of the difference power plan |
| DifferencePlanGuid | Guid | The GUID of the difference power plan |
| IncludeHidden | Switch | Whether to include hidden settings in the comparison |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

Returns a collection of PowerSetting objects that differ between the two plans.

### Reset-PowerPlanDefaults

Resets a power plan to its default settings.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Name | String | The name of the power plan to reset |
| Guid | Guid | The GUID of the power plan to reset |
| PassThru | Switch | Whether to return the reset power plan |
| UsePowrProf | Switch | Whether to use PowrProf.dll instead of WMI |

#### Output

If -PassThru is specified, returns the reset PowerPlan object.

### Get-PowerPlanStatsReport

Generates power plan usage statistics.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Days | Int | The number of days to include in the report |
| StartTime | DateTime | The start time for the report |
| EndTime | DateTime | The end time for the report |
| PlanName | String | The name of a specific power plan to report on |
| PlanGuid | Guid | The GUID of a specific power plan to report on |

#### Output

Returns one or more PowerPlanStats objects.

## Object Models

### PowerPlan

| Property | Type | Description |
|----------|------|-------------|
| Name | String | The name of the power plan |
| Guid | Guid | The GUID of the power plan |
| _IsActive | Boolean | Whether this power plan is active |
| Description | String | The description of the power plan |
| Settings | List<PowerSetting> | The list of power settings associated with this power plan |

### PowerSetting

| Property | Type | Description |
|----------|------|-------------|
| Alias | String | The friendly alias for the setting |
| SettingGuid | Guid | The GUID of the setting |
| SubGroupGuid | Guid | The GUID of the subgroup containing this setting |
| OnBattery | Object | The value when on battery power |
| PluggedIn | Object | The value when plugged in |
| Units | String | The units for the setting values |
| Description | String | The description of the setting |
| IsHidden | Boolean | Whether this setting is hidden |
| MinValue | Object | The minimum possible value for this setting |
| MaxValue | Object | The maximum possible value for this setting |
| PossibleValues | Object[] | The possible values for this setting (if enumeration) |

### PowerPlanStats

| Property | Type | Description |
|----------|------|-------------|
| PlanName | String | The name of the power plan |
| PlanGuid | Guid | The GUID of the power plan |
| TotalDuration | TimeSpan | The total duration this plan has been active |
| FirstSeen | DateTime | The first time this plan was seen active |
| LastSeen | DateTime | The last time this plan was seen active |
| PercentOfTimeActive | Double | The percentage of time this plan has been active |
| ActivationCount | Int | The number of times this plan has been activated |
