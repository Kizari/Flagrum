## Flagrum 1.1.13

_Released 20 April 2022_

### Changes

- A link is now displayed at the top of the file preview window showing the location of the EARC that file is  
  located in on disk. Clicking this link will open that folder in explorer
- Material locations are now displayed in the preview window for each mesh when selecting a gmdl.gfxbin file
- Environment Exporter/Importer has been updated to set up nodes for UV scaling  
  (will require a full export and import to apply)

### Bug Fixes

- Fixed bug where paths were not showing in the information window for custom presets
- Fixed bug where cloning a default preset would clone a custom preset instead
- Fixed bug where environment models without certain tags would fail to export
- Fixed bug where a couple of very rare materials would fail to read