# PowerPlanTools v2025.05.02.0851 Release Notes

## New Features

- Added centralized logging with timestamps and counters
- Added LoggingHelper class for consistent verbose output across all cmdlets
- Enhanced Find-PowerSetting cmdlet with case-insensitive regex and wildcard pattern support
- Added Import-PowerSettings cmdlet to import settings from JSON, CSV, and XML files
- Renamed Export-PowerSetting to Export-PowerSettings for consistency (plural form)
- Added automatic directory creation for export paths

## Improvements

- Standardized verbose logging format: 'MM/dd/yyyy HH:mm:ss.FFF - [counter/total] - message'
- Added detailed progress reporting for import/export operations
- Enhanced CSV export with plan metadata as comments
- Improved error handling and reporting
- Added support for creating new power plans during import if they don't exist

## Bug Fixes

- Fixed issue with verbose output showing when not requested
- Fixed directory creation for export paths
- Fixed CSV parsing for values containing commas

## Documentation

- Updated cmdlet help with new examples for regex and wildcard searches
- Added examples for the new Import-PowerSettings cmdlet
- Updated parameter descriptions for clarity
