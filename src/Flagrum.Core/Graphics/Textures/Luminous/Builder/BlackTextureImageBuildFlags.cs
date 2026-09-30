using System;

namespace Flagrum.Core.Graphics.Textures.Luminous.Builder;

[Flags]
public enum BlackTextureImageBuildFlags
{
    None = 0,
    InputCompressed = 1,
    OutputCompressed = 2,
    InputHDR = 4,
    OutputHDR = 8
}