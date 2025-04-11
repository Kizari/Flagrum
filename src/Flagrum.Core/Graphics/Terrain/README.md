# Format Information

### Height Entity Binary (*.heb)

This format is an image container format, similar to DDS. One or more images can be contained within, and the
pixel format varies based on the image type. The HEB files in the LOD folders generally contain images such as
heightmaps and merged mask maps (which control which textures go where on the terrain itself). The HEB files
in the diffuse and normal folders usually just contain one image at the resolution corresponding to the file
name that pertains to the name of the folder. Heightmaps are single channel 16-bit pixel information representing
an altitude value. The exact method of normalising these height values is not currently understood, but it can
be seen in the classes in this directory that there are values that would be used for this, such as `MinValue`,
`MaxValue`, and `AverageHeight`.

### Height Entity Physics (*.hephysx)

Not much is known about this format currently. It is assumed that it contains NVIDIA PhysX information for terrain
interaction (possibly things such as the footstep sounds to play). It is also assumed that it contains information
pertaining to the terrain collision (whether this is PhysX based or not is unknown).

### Terrain Palette Data (*.tpd)

This format describes which textures correspond to which pixel values on a merged mask map. It is not known
**exactly** how to map this correctly, but a seemingly correct result has been achieved in the experimental
terrain shader in Flagrum's Blender add-on by applying math operations to the values in the TPD file.
It would be best to refer to said code to learn more about how this works until further documentation is written.