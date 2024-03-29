﻿using System;

namespace Flagrum.Core.Gfxbin.Gmtl.Data;

public class MaterialSampler
{
    public string Name { get; set; }
    public ulong NameOffset { get; set; }

    public string ShaderGenName { get; set; }
    public ulong ShaderGenNameOffset { get; set; }

    public uint Flags { get; set; }
    public ulong Unknown { get; set; }

    public float MipmapLodBias { get; set; }
    public uint BorderColor { get; set; }
    public sbyte WrapS { get; set; }
    public sbyte WrapT { get; set; }
    public sbyte WrapR { get; set; }
    public Half MinLod { get; set; }
    public Half MaxLod { get; set; }
    public sbyte MaxAniso { get; set; }
    public sbyte MinFilter { get; set; }
    public sbyte MagFilter { get; set; }
    public sbyte MipFilter { get; set; }
    public sbyte Unknown1 { get; set; }
    public sbyte Unknown2 { get; set; }
    public sbyte Unknown3 { get; set; }
    public float UnknownR { get; set; }
    public float UnknownG { get; set; }
    public float UnknownB { get; set; }
    public float UnknownA { get; set; }
}