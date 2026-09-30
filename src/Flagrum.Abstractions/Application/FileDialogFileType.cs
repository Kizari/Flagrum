namespace Flagrum.Abstractions.Application;

/// <summary>
/// Represents a file type in a file dialog.
/// </summary>
public class FileDialogFileType(string name, string[] patterns)
{
    /// <summary>
    /// Name of the file type.
    /// </summary>
    public string Name { get; } = name;
    
    /// <summary>
    /// GLOB patterns that match the file type.
    /// </summary>
    public string[] Patterns { get; } = patterns;

    /// <summary>
    /// Matches all file types.
    /// </summary>
    public static FileDialogFileType All => new("All", ["*.*"]);

    /// <summary>
    /// Matches all image types that Flagrum supports.
    /// </summary>
    public static FileDialogFileType Images =>
        new("Image Files", ["*.png", "*.jpg", "*.jpeg", "*.tif", "*.tiff", "*.gif"]);

    /// <summary>
    /// Matches viable Flagrum mod types.
    /// </summary>
    public static FileDialogFileType AllFlagrumMods => new("Flagrum Mod", ["*.fmod", "*.zip"]);

    /// <summary>
    /// Matches the FMOD file format.
    /// </summary>
    public static FileDialogFileType FlagrumMod => new("Flagrum Mod", ["*.fmod"]);

    /// <summary>
    /// Matches game model files.
    /// </summary>
    public static FileDialogFileType GameModel => new("Game Model", ["*.gmdl.gfxbin"]);

    /// <summary>
    /// Matches Windows executable files.
    /// </summary>
    public static FileDialogFileType WindowsExecutable => new("Windows Executable", ["*.exe"]);

    /// <summary>
    /// Matches the Steam Workshop mod list file type.
    /// </summary>
    public static FileDialogFileType BinmodList => new("Workshop Mod List", ["*.list"]);

    /// <summary>
    /// Matches Flagrum model data files exported by Flagrum-Blender.
    /// </summary>
    public static FileDialogFileType FlagrumModelData => new("Flagrum Model Data", ["*.fmd"]);

    /// <summary>
    /// Matches Flagrum environment data files exported by Flagrum.
    /// </summary>
    public static FileDialogFileType FlagrumEnvironmentData => new("Flagrum Environment Data", ["*.fed"]);

    /// <summary>
    /// Matches Flagrum terrain data files exported by Flagrum.
    /// </summary>
    public static FileDialogFileType FlagrumTerrainData => new("Flagrum Terrain Data", ["*.ftd"]);

    /// <summary>
    /// Matches Luminous Engine Graphics Binary files.
    /// </summary>
    public static FileDialogFileType GraphicsBinary => new("Graphics Binary", ["*.gfxbin"]);

    /// <summary>
    /// Matches PNG images.
    /// </summary>
    public static FileDialogFileType PortableNetworkGraphic => new("Portable Network Graphic", ["*.png"]);

    /// <summary>
    /// Matches TGA files.
    /// </summary>
    public static FileDialogFileType Targa => new("Targa", ["*.tga"]);

    /// <summary>
    /// Matches DDS files.
    /// </summary>
    public static FileDialogFileType DirectDrawSurface => new("DirectDraw Surface", ["*.dds"]);

    /// <summary>
    /// Matches BTEX files.
    /// </summary>
    public static FileDialogFileType BlackTexture => new("Black Texture", ["*.btex"]);

    /// <summary>
    /// Matches XML files.
    /// </summary>
    public static FileDialogFileType Xml => new("eXtensible Markup Language", ["*.xml"]);

    /// <summary>
    /// Matches EXML files.
    /// </summary>
    public static FileDialogFileType EbonyXml => new("Ebony XML", ["*.exml"]);

    /// <summary>
    /// Matches HEB files.
    /// </summary>
    public static FileDialogFileType HeightEntityBinary => new("Height Entity Binary", ["*.heb"]);

    /// <summary>
    /// File types that are compatible with Flagrum's texture functionality.
    /// </summary>
    public static FileDialogFileType[] Textures => [PortableNetworkGraphic, Targa, DirectDrawSurface, BlackTexture];

    /// <summary>
    /// File types that are compatible with Flagrum's terrain texture functionality.
    /// </summary>
    public static FileDialogFileType[] TerrainTextures =>
        [PortableNetworkGraphic, Targa, DirectDrawSurface, HeightEntityBinary];

    /// <summary>
    /// File types that are compatible with Flagrum's EXML functionality.
    /// </summary>
    public static FileDialogFileType[] EbonyXmlFiles => [Xml, EbonyXml];
}