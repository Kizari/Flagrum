## Flagrum 1.1.14

_Released 28 April 2022_

### Changes

- Blender importers now handle both blue and yellow normal maps automatically
- Moving props are now exported when using Export as Environment
- Asset list can now be navigated with the up/down arrows

### Bug Fixes

- Fixed bug where Environment Import would sometimes fail due to UV scaling
- Fixed bug where Environment Exporter would occasionally crash Flagrum
- Fixed bug where multiple extensions would sometimes appear on exported models

> **NOTE**  
> - You will need to re-export any environments if you want the extra moving props  
>   (should that environment have any to export).   
> - You will not need to re-export to take advantage of the normal maps changes or UV scaling fix,  
>   but you will need to re-import.