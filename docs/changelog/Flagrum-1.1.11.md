## Flagrum 1.1.11

_Released 14 April 2022_

### Changes

- Added automatic node setup for opacity mask textures
- Added notification for when environment exports are complete
- Added button to disconnect/reconnect all emission textures for rendering
- Enabled backface culling on all environment materials to improve performance
- Removed automatic node setup for emission on environments when texture is blank

### Bug Fixes

- Fixed rotation issues with exported environments
- Fixed bug where environments would accumulate if not restarting between exports
- Fixed bug where environment export would crash when encountering an invalid URI
- Fixed bug where environment import would occasionally fail on certain textures
- Fixed bug where texture preview would crash Flagrum on File System view
- Fixed edge-case where certain imports would fail due to non-numeric vertex coords