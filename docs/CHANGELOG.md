# Changelog

All notable changes to the PowerPlanTools module will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2025.05.03.1730] - 2025-05-03

### Added
- Added comprehensive power setting aliases for all known power settings
- Enhanced subgroup GUID-to-alias mapping for better organization
- Added Find-SubGroup cmdlet to search for power subgroups
- Added SubGroupAlias property to PowerSetting class
- Updated Find-PowerSetting and Get-PowerPlan to include SubGroupAlias in output
- Added Get-PowerState and Set-PowerState cmdlets for managing power state settings
- Improved documentation with examples for new features
- Added support for power setting possible values with FriendlyName and ActualValue properties
- Added default possible values for common power settings

### Changed
- Improved parameter naming consistency across cmdlets
- Enhanced verbose logging with timestamps and counters
- Always include settings when getting power plans (no opt-in required)
- Always include possible values for power settings
- Completely removed WMI usage in favor of Windows Power Management API

### Fixed
- Fixed duplicate verbose messages in cmdlets that use ShouldProcess

## [2025.05.01.0000] - 2025-05-01

### Added
- Initial release of PowerPlanTools
- Support for managing Windows power plans and settings
- 10 cmdlets for power plan management
- Support for both WMI and PowrProf.dll APIs
- Support for exporting and importing power plans
- Support for power plan statistics
- Support for hidden power settings
