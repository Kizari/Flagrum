using System;
using System.Linq;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Core.Graphics.Models;

public class VertexElementSemantic : Enum<string>
{
    private VertexElementSemantic(string value) : base(value) { }

    public static VertexElementSemantic Position0 => new("POSITION0");
    public static VertexElementSemantic Normal0 => new("NORMAL0");
    public static VertexElementSemantic Color0 => new("COLOR0");
    public static VertexElementSemantic Color1 => new("COLOR1");
    public static VertexElementSemantic Color2 => new("COLOR2");
    public static VertexElementSemantic Color3 => new("COLOR3");
    public static VertexElementSemantic TexCoord0 => new("TEXCOORD0");
    public static VertexElementSemantic TexCoord1 => new("TEXCOORD1");
    public static VertexElementSemantic TexCoord2 => new("TEXCOORD2");
    public static VertexElementSemantic TexCoord3 => new("TEXCOORD3");
    public static VertexElementSemantic TexCoord4 => new("TEXCOORD4");
    public static VertexElementSemantic TexCoord5 => new("TEXCOORD5");
    public static VertexElementSemantic TexCoord6 => new("TEXCOORD6");
    public static VertexElementSemantic TexCoord7 => new("TEXCOORD7");
    public static VertexElementSemantic BlendWeight0 => new("BLENDWEIGHT0");
    public static VertexElementSemantic BlendWeight1 => new("BLENDWEIGHT1");
    public static VertexElementSemantic BlendIndices0 => new("BLENDINDICES0");
    public static VertexElementSemantic BlendIndices1 => new("BLENDINDICES1");
    public static VertexElementSemantic Tangent0 => new("TANGENT0");
    public static VertexElementSemantic Tangent1 => new("TANGENT1");
    public static VertexElementSemantic Binormal0 => new("BINORMAL0");
    public static VertexElementSemantic Binormal1 => new("BINORMAL1");
    public static VertexElementSemantic Normal2Factors => new("NORMAL2FACTORS0");
    public static VertexElementSemantic Normal4Factors => new("NORMAL4FACTORS0");
    
    /// <remarks>
    /// Only known to be present in Episode Duscae.
    /// </remarks>
    public static VertexElementSemantic TangentFactor0 => new("TANGENTFACTOR0");
    
    /// <remarks>
    /// Only known to be present in Episode Duscae.
    /// </remarks>
    public static VertexElementSemantic TangentFactor1 => new("TANGENTFACTOR1");
    
    /// <remarks>
    /// Only known to be present in Episode Duscae.
    /// </remarks>
    public static VertexElementSemantic BinormalFactor0 => new("BINORMALFACTOR0");
    
    /// <remarks>
    /// Only known to be present in Episode Duscae.
    /// </remarks>
    public static VertexElementSemantic BinormalFactor1 => new("BINORMALFACTOR1");
    
    public static VertexElementSemantic FogCoord0 => new("FOGCOORD0");
    public static VertexElementSemantic PSize0 => new("PSIZE0");

    public static explicit operator VertexElementSemantic(string name)
    {
        var result = GetAll<VertexElementSemantic>().FirstOrDefault(e => e.Value == name);
        if (result == null)
        {
            throw new ArgumentException($"Unknown {nameof(VertexElementSemantic)} \"{name}\".", nameof(name));
        }

        return result;
    }

    public static explicit operator string(VertexElementSemantic semantic)
    {
        return semantic.Value;
    }
}