# Flagrum.Blender

This project is the Blender add-on used for creating FMD files for Flagrum's Mod Library functionality.

### C# Console Application

This component of the project is a simple console application that is called from the Blender Python scripts to leverage
parts of the core Flagrum library. This is done to prevent writing and maintaining a second copy of the same code in a
different language so we can focus our efforts on bigger priorities.

### Python Add-on

The rest of the project are the Python scripts that make up the Blender add-on. These scripts are copied to the publish
directory by MSBuild to ensure the entire setup can be published with a single click.