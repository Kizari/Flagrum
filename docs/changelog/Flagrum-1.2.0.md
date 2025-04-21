## Flagrum 1.2.0

_Released 22 May 2022_

### New Features

- EARC Mod Manager has been added
>**Introducing the Mod Manager**   
> Install and uninstall mods with a single click, create EARC mods with ease, import legacy mods, and share your mods with the community! [Learn More](https://github.com/Kizari/Flagrum/wiki/Mod-Manager)
- Terrain Exporter as been added
>**Introducing the Terrain Exporter**   
>For the first time ever, we can now export terrain from FFXV! [Learn More](https://github.com/Kizari/Flagrum/wiki/Terrain-Exporter)

**Thanks so much to @Katelynn Kittaly , @Rinual , and @NightysWolf for their efforts in helping me with this update.**

### Changes

- BTEX support has been extended to cover textures that were previously unreadable by any of the BTEX tools
- Transformations are now automatically applied when exporting to FMD
- Seam normals are now automatically corrected when exporting to FMD
- Tutorials tab has been replaced with Wiki tab to keep the information up-to-date

### Bug Fixes

- Fixed bug where creating a new Model Replacement Preset would return a 404 error
- Fixed bug where Flagrum would crash when subscribing to your own Workshop Mods
- Workshop Mods will no longer deep fry sRGB thumbnails