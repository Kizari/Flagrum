## Flagrum 1.3.3 and 1.3.4

_Released 22 December 2022_

### New Features

- Address bar navigation is now available in the Mod Manager popup Asset Explorer
- Files can now be switched in the Mod Manager build list without creating a new build instruction
- Build list instructions in the Mod Manager can now be filtered by a search query
- An icon has been added at the top of Asset Explorer while in the root folder to allow bulk export directly from data://
- Texture settings have been added to the 3D viewer so the user can decide on the load speed versus quality trade-off
- Tree view is now available in Asset Explorer's Game View for those that prefer this style
- List view is now available in Asset Explorer's File System for those that prefer this style
- Virtualisation has been added to the Mod Manager build list to speed up the UI on larger mods
- File System view in Asset Explorer now supports a larger range of previews and conversions

### Changes

- Build order of reverting mods has been altered to prevent corruption in cases of rare crashes
- Export Folder has been reworked to allow a much more versatile range of export options
- Text is now selected when clicking on the address bar in Asset Explorer to make pasting URIs quicker
- The popup Asset Explorer now stays visible while selecting a replacement file in the Mod Manager
- Copyguard protection has been added to Workshop mods in hopes it may allow updating of old MO mods
- Texture replacements now support DDS files once again
- Flagrum's icon has been updated. Thanks to Luthus Nox Fleuret for creating the logo and associated graphics

### Bug Fixes

- Fixed a bug where non-Flagrum Workshop mods were being corrupted when altering the stats—[#10](https://github.com/Kizari/Flagrum/issues/10)
- Fixed BTEX converter crash when replacing textures with non-compressed pixel formats
- Fixed a bug where file association for FMOD was bound to older versions of Flagrum
- Fixed a glitch with the Asset Explorer for model replacement presets— [#32](https://github.com/Kizari/Flagrum/issues/32)
- Fixed a rare bug where changing model previews could crash Flagrum— [#33](https://github.com/Kizari/Flagrum/issues/33)
- Fixed a rare bug where Flagrum could crash when switching to Asset Explorer— [#21](https://github.com/Kizari/Flagrum/issues/21)
- Fixed a bug where some broken models would crash the 3D viewer— [#18](https://github.com/Kizari/Flagrum/issues/18)
- Fixed bug where loading indicator would not show when switching between model previews
