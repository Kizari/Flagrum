using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Xunit;

namespace Flagrum.Test.Graphics;

public class MaterialTests
{
    [Fact]
    public void VerifyReaderWriter()
    {
        var materialPath = Path.Combine(IOHelper.GetExecutingDirectory(), "Graphics", "Resources",
            "nh00_010_cloth_00_mat.gmtl.gfxbin");

        var materialData = File.ReadAllBytes(materialPath);
        var material = new GameMaterial();
        material.Read(materialData);

        var material2Data = material.Write();
        var material2 = new GameMaterial();
        material2.Read(materialData);

        Assert.True(material.DeepCompare(material2));
        Assert.True(materialData.HashCompare(material2Data));
    }
}