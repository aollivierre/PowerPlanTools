# PowerPlanTools

PowerPlanTools is a PowerShell Binary Module for native management of Windows Power Plans and Power Settings.

## Features

* Native power plan and setting manipulation using WMI and PowrProf.dll (no powercfg.exe)
* Full alias support for known power setting GUIDs
* Optional raw GUID input for advanced use
* Tab-completion for PlanName, SettingAlias, and SettingGuid
* Export/Import of power plans using native API
* Power plan statistics over time using event logs
* Reports in JSON, CSV, or XML
* Full metadata output (`-WithMetadata`)
* Support for hidden settings (`-IncludeHidden`)
* Pipeline support for all relevant cmdlets
* Full support for `-WhatIf`, `-Confirm`, `-Force`

## Installation

See [INSTALL.md](docs/INSTALL.md) for detailed installation instructions.

## Usage

See [USAGE.md](docs/USAGE.md) for detailed usage instructions and examples.

## Documentation

* [CHANGELOG.md](docs/CHANGELOG.md) - Version history and changes
* [INSTALL.md](docs/INSTALL.md) - Installation instructions
* [USAGE.md](docs/USAGE.md) - Usage instructions and examples
* [API-SPEC.md](docs/API-SPEC.md) - API specification
* [MODULE-STRUCTURE.md](docs/MODULE-STRUCTURE.md) - Module structure documentation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
