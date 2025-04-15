## Flagrum 1.2.4

_Released 8 August 2022_

### New Features

- Right-click option added to folders in Asset Explorer that allows exporting all script files to XML or XMB2
- Mod Manager card thumbnails are now automatically resized when set to reduce mod size
- Existing thumbnails will be automatically resized to save disk space
- Flagrum now supports installing retexture mods that include 4K textures when the 4K pack is not installed
- Legacy mods with files targeting the 4K packs can also be installed now if the 4K pack is missing

### Bug Fixes

- XML now exports correctly on devices that use a comma decimal separator
- Menu graphics are no longer deep fried when automatically converted to BTEX by the Mod Manager
- Fixed issue where legacy mods with multiple conflicts would fail to install after selecting the first item to keep
- Mods that target files you do not have will no longer potentially leave your files in a bad state
- Delete button no longer shows on New EARC Mod page (use the cancel button instead)
- Loading indicator now shows correctly after acknowledging a mod conflict when enabling mods
- Loading indicator no longer gets stuck on indefinitely when declining to disable a conflicting mod
- Fixed bug where Flagrum would rarely crash randomly when using the Mod Manager due to database thread collision
- Mod thumbnails now update correctly when switching between different EARC mods
- Mod thumbnails now update correctly on the mod card after changing the image
- Fixed a rare issue where Workshop Mods would not show up due to case sensitive file paths