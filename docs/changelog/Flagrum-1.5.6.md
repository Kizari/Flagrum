## Flagrum 1.5.6

_Released 28 January 2024_

### New Features

- Loose files can now be indexed by Flagrum
- HEB terrain files can now be previewed and exported

### Changes

- Updated all projects to .NET8 and removed Windows toast from WpfService and environment exporter
- Added a continuous deployment workflow for Flagrum
- Migrated the last of the data out of SQLite
- Minor refactoring throughout Flagrum and added new documentation files

### Bug Fixes

- Terrain exporter had a bug that has been fixed
- Fixed duplicate workshop mod crash- [#120](https://github.com/Kizari/Flagrum/issues/120)
- Fixed issue where 3D viewer keybinds were displaying as numbers instead of readable names
- Fixed crash with models that don't have gpubins 
- Fixed bug where Flagrum would crash when trying to resolve URIs for files that don't exist
- Fixed issue where commandline args would repeat when restarting Flagrum via profile change or language change- [#81](https://github.com/Kizari/Flagrum/issues/81)
- Included missing file extensions for Forspoken and fixed dual extension issue- [#89](https://github.com/Kizari/Flagrum/issues/89)
- Fixed issue where Forspoken BTEX would not export nor preview correctly- [#125](https://github.com/Kizari/Flagrum/issues/125)