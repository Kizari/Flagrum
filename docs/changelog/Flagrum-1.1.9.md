## Flagrum 1.1.9

_Released 11 April 2022_

### New Features

- Flagrum's Asset Explorer can now preview and export EXML files as XML within the application
- GFXBIN importer for Blender now automatically sets up the materials and node graphs for your models on import,  
  provided the relevant files are there.
- Introduced the Environment Exporter! A great tool for exploring the world  

> **NOTE**   
> The environment exporter is now ready for use!  
> There's a great write-up on it 
> [at the following link](https://github.com/Kizari/Flagrum/wiki/Environment-Exporter), 
> including some caveats to watch out for.

- EXML files can now be exported as Flagrum Environment Data (FED)

### Bug Fixes

- Fixed bug where 4K textures were not being exported via Export with Dependencies
- Fixed bug where some edge-case models could not be imported into Blender due to broken UV coordinates