# Format Information

### Graphics Binary (*.\<Type\>.gfxbin)

This format is just a container for various graphics formats. Luminous currently only uses this for Game Model (gmdl)
and Game Material (gmtl) as far as is known. The format is comprised of:

* Version
* Dependency Table
* Hash List

The dependency table is a dictionary with `uint64` hashes represented as strings as the keys.
The values are the file URIs that these hashes represent.
However, there is an exception to this. Each GFXBIN appears to have a `ref` key which contains the URI
of the GFXBIN file itself, and an `asset_uri` key which contains the URI to its containing folder.

The hash list is simply a `uint64` list of the hashes from the dependency table, less the `ref` and `asset_uri` keys.