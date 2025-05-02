@{
    # Script module or binary module file associated with this manifest.
    RootModule = 'lib\PowerPlanTools.dll'

    # Version number of this module.
    ModuleVersion = '2025.05.02.1542'

    # Supported PSEditions
    CompatiblePSEditions = @('Desktop', 'Core')

    # ID used to uniquely identify this module
    GUID = '9b5b897e-8c97-4e85-9c8c-7a9b51a7b346'

    # Author of this module
    Author = 'Grace Solutions'

    # Company or vendor of this module
    CompanyName = 'Grace Solutions'

    # Copyright statement for this module
    Copyright = '(c) 2025 Grace Solutions. All rights reserved.'

    # Description of the functionality provided by this module
    Description = 'PowerShell module for managing Windows power plans and power settings'

    # Minimum version of the Windows PowerShell engine required by this module
    PowerShellVersion = '5.1'

    # Name of the Windows PowerShell host required by this module
    # PowerShellHostName = ''

    # Minimum version of the Windows PowerShell host required by this module
    # PowerShellHostVersion = ''

    # Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
    DotNetFrameworkVersion = '4.7.2'

    # Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
    CLRVersion = '4.0'

    # Processor architecture (None, X86, Amd64) required by this module
    # ProcessorArchitecture = ''

    # Modules that must be imported into the global environment prior to importing this module
    # RequiredModules = @()

    # Assemblies that must be loaded prior to importing this module
    RequiredAssemblies = @('lib\PowerPlanTools.dll', 'lib\Newtonsoft.Json.dll')

    # Script files (.ps1) that are run in the caller's environment prior to importing this module.
    # ScriptsToProcess = @()

    # Type files (.ps1xml) to be loaded when importing this module
    TypesToProcess = @('types\PowerPlanTools.Types.ps1xml')

    # Format files (.ps1xml) to be loaded when importing this module
    FormatsToProcess = @('types\PowerPlanTools.Format.ps1xml')

    # Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
    # NestedModules = @()

    # Functions to export from this module
    FunctionsToExport = @()

    # Cmdlets to export from this module
    CmdletsToExport = @(
        'Compare-PowerPlans',
        'Export-PowerSettings',
        'Find-PowerSetting',
        'Get-PowerPlan',
        'Get-PowerPlanStatistic',
        'Get-PowerSetting',
        'Get-PowerState',
        'Import-PowerSettings',
        'New-PowerPlan',
        'Remove-PowerPlan',
        'Reset-PowerPlanDefaults',
        'Set-PowerPlan',
        'Set-PowerState',
        'Update-PowerSetting'
    )

    # Variables to export from this module
    VariablesToExport = '*'

    # Aliases to export from this module
    AliasesToExport = @()

    # DSC resources to export from this module
    # DscResourcesToExport = @()

    # List of all modules packaged with this module
    # ModuleList = @()

    # List of all files packaged with this module
    # FileList = @()

    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{
        PSData = @{
            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('Power', 'Battery', 'Settings', 'WMI')

            # A URL to the license for this module.
            LicenseUri = 'https://opensource.org/licenses/MIT'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/Grace-Solutions/PowerPlanTools'

            # A URL to an icon representing this module.
            # IconUri = ''

            # ReleaseNotes of this module
            ReleaseNotes = @'
- Added centralized logging with timestamps and counters
- Enhanced Find-PowerSetting cmdlet with case-insensitive regex and wildcard pattern support
- Added Import-PowerSettings cmdlet to import settings from JSON, CSV, and XML files
- Renamed Export-PowerSetting to Export-PowerSettings for consistency (plural form)
- Added automatic directory creation for export paths
- Changed _IsActive property to IsActive for better naming convention
- Made UsePowrProf opt-out instead of opt-in (renamed to UseWmi)
- Added Get-PowerState and Set-PowerState cmdlets for managing power state settings
'@
        }
    }

    # HelpInfo URI of this module
    # HelpInfoURI = ''

    # Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
    # DefaultCommandPrefix = ''
}
