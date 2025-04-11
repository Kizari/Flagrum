using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Utilities.Exceptions;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Mod.Legacy;

namespace Flagrum.Application.Features.ModManager.Mod;

public class FlagrumModPack : IDisposable
{
    public const uint CurrentVersion = 4;
    public const string DefaultMagic = "FMOD";

    private FileStream _stream;

    public FlagrumModPack() { }

    public FlagrumModPack(string path)
    {
        _stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public string Magic { get; set; } = DefaultMagic;
    public uint Version { get; set; } = CurrentVersion;
    public uint Count { get; set; }
    public List<FlagrumMod> Mods { get; set; } = new();

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _stream?.Dispose();
    }

    public void Read(string path, IModBuildInstructionFactory factory)
    {
        using var reader = new BinaryReader(_stream, Encoding.UTF8, true);

        Magic = reader.ReadString(4);
        if (Magic != DefaultMagic)
        {
            throw new FileFormatException("Input file was not a valid FMOD file");
        }

        Version = reader.ReadUInt32();
        if (Version > CurrentVersion)
        {
            throw new FormatVersionException("FMOD version newer than current supported version");
        }

        // Handle older versions of FMOD
        if (Version < 4)
        {
            _stream.Dispose();
            Fmod3.ToFlagrumModPack(path, this, factory);
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return;
        }

        Count = reader.ReadUInt32();
        _stream.Align(16);

        for (var i = 0; i < Count; i++)
        {
            var mod = new FlagrumMod();
            mod.Read(reader, factory);
            Mods.Add(mod);
        }
    }

    public void Write(string path)
    {
        Count = (uint)Mods.Count;

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        writer.Write(Magic.ToCharArray());
        writer.Write(Version);
        writer.Write(Count);
        writer.Align(16, 0x00);

        foreach (var mod in Mods)
        {
            mod.Write(writer);
        }
    }
}