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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

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
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

#### Output

If -PassThru is specified, returns the reset PowerPlan object.

### Get-PowerPlanStatistic

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

### Find-PowerSetting

Searches for power settings by name, description, or GUID.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| PlanName | String | The name of the power plan to search in |
| PlanGuid | Guid | The GUID of the power plan to search in |
| SearchString | String | The search string to find in power setting names or descriptions |
| Regex | Switch | Whether to use regex pattern matching for the search string |
| Wildcard | Switch | Whether to use wildcard pattern matching for the search string |
| GuidPattern | String | The GUID pattern to search for |
| SearchIn | String | Where to search for the search string (Name, Description, Both) |
| Hidden | Switch | Whether to search for hidden settings only |
| IncludeHidden | Switch | Whether to include hidden settings in the search results |
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

#### Output

Returns one or more PowerSetting objects.

### Find-SubGroup

Searches for power subgroups by name or GUID.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Name | String | The name pattern to search for (supports wildcards and regex) |
| SubGroupGuid | String | The GUID pattern to search for (supports wildcards and regex) |
| Regex | Switch | Whether to use regex pattern matching instead of wildcards |

#### Output

Returns one or more SubGroupInfo objects.

### Get-PowerState

Gets the current power state settings.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Property | String | The specific property to retrieve (HibernateEnabled, ConnectedStandbyEnabled, etc.) |
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

#### Output

Returns a PowerStateInfo object.

### Set-PowerState

Configures power state settings.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| EnableHibernate | Boolean | Whether to enable or disable hibernation |
| EnableConnectedStandby | Boolean | Whether to enable or disable connected standby |
| EnableS3 | Boolean | Whether to enable or disable S3 sleep state |
| EnableFastStartup | Boolean | Whether to enable or disable fast startup |
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

#### Output

None.

### Export-PowerSettings

Exports power settings to a file.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Name | String | The name of the power plan |
| PlanGuid | Guid | The GUID of the power plan |
| Path | String | The path to the output file |
| Format | String | The output format (Json, Csv, Xml) |
| IncludeHidden | Switch | Whether to include hidden settings in the output |
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

#### Output

None.

### Import-PowerSettings

Imports power settings from a file.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Path | String | The path to the input file |
| Name | String | The name of the target power plan |
| PlanGuid | Guid | The GUID of the target power plan |
| CreateIfNotExists | Switch | Whether to create the power plan if it doesn't exist |
| UseWmi | Switch | Whether to use WMI instead of PowrProf.dll |

#### Output

Returns the PowerPlan object if successful.

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
| SubGroupAlias | String | The friendly alias for the subgroup |
| OnBattery | Object | The value when on battery power |
| PluggedIn | Object | The value when plugged in |
| Units | String | The units for the setting values |
| Description | String | The description of the setting |
| IsHidden | Boolean | Whether this setting is hidden |
| MinValue | Object | The minimum possible value for this setting |
| MaxValue | Object | The maximum possible value for this setting |
| PossibleValues | Object[] | The possible values for this setting (if enumeration) |
| PlanName | String | The name of the power plan this setting belongs to |
| PlanGuid | Guid | The GUID of the power plan this setting belongs to |

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

### SubGroupInfo

| Property | Type | Description |
|----------|------|-------------|
| SubGroupGuid | Guid | The GUID of the subgroup |
| Name | String | The friendly name of the subgroup |

### PowerStateInfo

| Property | Type | Description |
|----------|------|-------------|
| HibernateEnabled | Boolean | Whether hibernation is enabled |
| ConnectedStandbyEnabled | Boolean | Whether connected standby is enabled |
| S3Enabled | Boolean | Whether S3 sleep state is enabled |
| FastStartupEnabled | Boolean | Whether fast startup is enabled |
| HybridSleepEnabled | Boolean | Whether hybrid sleep is enabled |
