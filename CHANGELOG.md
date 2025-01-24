# Changelog

All notable changes to this project will be documented in this file.  
&nbsp;

## Flagrum 1.5.17

_Released 24 January 2025_


### Bug Fixes

- Rework mod loader ([@Kizari](https://github.com/Kizari))


### Refactor

- Remove redundant legacy code ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.16

_Released 20 December 2024_


### Bug Fixes

- Resolve unpersisted gift token state ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.15

_Released 17 December 2024_


### Bug Fixes

- Rework Flagrum+ authentication ([@Kizari](https://github.com/Kizari))


### Refactor

- Fix namespace issues ([@Kizari](https://github.com/Kizari))

- Remove Lucent IPC ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.14

_Released 18 June 2024_


### Bug Fixes

- Hotfix for file system view ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.13

_Released 15 June 2024_


### New Features

- Add new file finder ([@Kizari](https://github.com/Kizari))

- Add property browser for assets ([@Kizari](https://github.com/Kizari))


### Bug Fixes

- Fix colorSet2 regression for workshop mods ([@Kizari](https://github.com/Kizari))

- Resolve misnaming issue for Forspoken—
[#151](https://github.com/Kizari/Flagrum/issues/151) ([@Kizari](https://github.com/Kizari))

- Resolve file index duplicates issue—
[#150](https://github.com/Kizari/Flagrum/issues/150), [#152](https://github.com/Kizari/Flagrum/issues/152) ([@Kizari](https://github.com/Kizari))

- Add missing PS4 sound extensions ([@Kizari](https://github.com/Kizari))

- Prevent crash when deleting uncached mods—
[#141](https://github.com/Kizari/Flagrum/issues/141) ([@Kizari](https://github.com/Kizari))

- Repair file index inconsistencies ([@Kizari](https://github.com/Kizari))

- Solve environment export texture resolution—
[Kizari/Flagrum#149](https://github.com/Kizari/Flagrum/issues/Kizari/Flagrum#149) ([@Kizari](https://github.com/Kizari))


### Refactor

- Add BlackTexture enums ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.12

_Released 22 February 2024_


### New Features

- Add backward-compatibility support for Lucent ([@Kizari](https://github.com/Kizari))


### Bug Fixes

- Resolve broken model replacement preset UI ([@Kizari](https://github.com/Kizari))

- Prevent file indexing spinner from persisting ([@Kizari](https://github.com/Kizari))

- Fix automatic colorSet2 for workshop mods ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.11

_Released 14 February 2024_


### New Features

- Add Forspoken support to force reset feature ([@Kizari](https://github.com/Kizari))


### Bug Fixes

- Improve formatting for automated release notes ([@Kizari](https://github.com/Kizari))

- Resolve broken crash log functionality ([@Kizari](https://github.com/Kizari))

- Fix missing data issues with some applied mods ([@Kizari](https://github.com/Kizari))

- Prevent FMOD file lock persisting ([@Kizari](https://github.com/Kizari))

- Resolve new archive add file refresh issue ([@Kizari](https://github.com/Kizari))

- Prevent binmod.list corrupting from broken mods ([@Kizari](https://github.com/Kizari))


### Refactor

- Move mod state into ModManager service ([@Kizari](https://github.com/Kizari))


&nbsp;
## Flagrum 1.5.10

_Released 12 February 2024_


### New Features

- Add PKA extraction to File System view ([@Kizari](https://github.com/Kizari))

- Add release notes fetch and display ([@Kizari](https://github.com/Kizari))

- Upgrade file logging system to use Serilog ([@Kizari](https://github.com/Kizari))

- Support generics in automatic constructors ([@Kizari](https://github.com/Kizari))


### Bug Fixes

- Fix HEB exporter crash caused by empty files ([@Kizari](https://github.com/Kizari))

- Fix HEB preview crash caused by empty files ([@Kizari](https://github.com/Kizari))

- Ensure HEB resolution displays in file name ([@Kizari](https://github.com/Kizari))

- Warn user when nothing exported from empty HEB ([@Kizari](https://github.com/Kizari))

- Resolve PKA export crash in File System view ([@Kizari](https://github.com/Kizari))

- Fix merged archive corruption issue ([@Kizari](https://github.com/Kizari))

- Resolve rare data migration issue ([@Kizari](https://github.com/Kizari))

- Update bugged delta updater package ([@Kizari](https://github.com/Kizari))


&nbsp;

&nbsp;
---
*This changelog is automatically generated from commit history by a script.*
