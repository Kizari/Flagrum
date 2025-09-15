using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Core.Data.Binary;

namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <summary>
/// Header for <see cref="BlackTexture" />.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BlackTextureHeader
{
    /// <summary>
    /// Expected value for <see cref="SectionDataBinary.Subtype" /> when the <see cref="SectionDataBinary" />
    /// contains a <see cref="BlackTexture" /> resource.
    /// </summary>
    public const uint SedbType = 0x78657462; // "btex"

    /// <summary>
    /// Expected value for <see cref="Magic" />.
    /// </summary>
    public const uint MagicValue = 0x58455442; // "BTEX"

    public static int StructSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<BlackTextureHeader>();
    }

    /// <summary>
    /// Identifier for the Black Texture file format.
    /// </summary>
    public uint Magic;

    /// <summary>
    /// Size of this header, in bytes.
    /// </summary>
    public uint HeaderSize;

    /// <summary>
    /// Size of the raw pixel information, in bytes.
    /// </summary>
    public uint ImageDataSize;

    /// <summary>
    /// Version of the BTEX file format the file was written with.
    /// </summary>
    public BlackTextureVersion Version;

    /// <summary>
    /// Platform that the texture is targeted towards.
    /// </summary>
    public BlackTexturePlatform Platform;

    /// <summary>
    /// Attributes that apply to this texture.
    /// </summary>
    public BlackTextureFlags Flags;

    /// <summary>
    /// Number of images in this texture.
    /// </summary>
    public ushort ImageCount;

    /// <summary>
    /// Size of <see cref="BlackTextureImageHeader" />.
    /// </summary>
    public ushort ImageHeaderStride;

    /// <summary>
    /// Offset of the first <see cref="BlackTextureImageHeader" /> relative to the start of this struct.
    /// </summary>
    public uint ImageHeaderOffset;

    /// <summary>
    /// Useless padding, ignore this.
    /// </summary>
    public ulong Reserved;
}