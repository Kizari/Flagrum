## Flagrum 1.2.3

_Released 13 June 2022_

### New Features

- Additional information has been added to legacy mod installation errors to help understand why the mod will not install

### Changes

- XML preview and export will now automatically encode reserved symbols—this fixes issues where certain EXML files would cause the game to crash when put back into the game

### Bug Fixes

- Mods with instructions to "Remove" a file can now be saved successfully
- Re-enabling a mod with "Remove" instructions will no longer permanently break the original EARC
- Legacy mods will now be installed correctly when an existing mod removes files that the legacy mod would need to alter
- Mods with "Remove" instructions can now be exported successfully
- Installing legacy mods with multiple conflicts will no longer crash Flagrum
- Warning text that appears when mods are enabled on the "Rebuild File Index" dialog is no longer truncated
