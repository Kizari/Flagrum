## Flagrum 1.3.5

_Released 15 January 2023_

### New Features

- Added localisation for Asset Explorer Settings

### Changes

- Environment exporter is now a reasonable bit faster

### Bug Fixes

- Fixed a bug where non-recursive folder export would not respect the file type filter
- Fixed a bug where "unknown" file types were not exporting with their "true" extension
- Fixed a bug where the Asset Explorer would always show loading message on fresh installs—
  [#38](https://github.com/Kizari/Flagrum/issues/38)
- Fixed a bug where the 3D model preview would sit outside the Flagrum window on some devices—
  [#17](https://github.com/Kizari/Flagrum/issues/17)