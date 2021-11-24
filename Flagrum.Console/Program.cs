using System.IO;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        //var uri = "data://mod/noctis_custom_2/main.fbxgmtl/button.gmtl";
        //System.Console.WriteLine(Cryptography.Hash("button.gmtl.gfxbin"));
        InteropTests.Test();
        //ArchiverTests.GetShader();
        //var bytes = EntityPackage.BuildExml("noctis_custom", "clean");
        //File.WriteAllBytes("C:\\Testing\\temp2.exml", bytes);
        //MaterialsToTemplates.Run();
        // CsvToPython.Parse(
        //     "C:\\Testing\\Gfxbin\\Gmtl\\material_data.csv",
        //     "C:\\Testing\\Gfxbin\\Gmtl\\material_data_input_importance.csv",
        //     "C:\\Testing\\Gfxbin\\Gmtl\\material_data_texture_importance.csv",
        //     "C:\\Testing\\Gfxbin\\Gmtl\\material_data_original_names.csv");
        //GfxbinTests.PrintData();
        //MaterialTests.BuildSkinMaterial();
        //GfxbinTests.Export();
        //GfxbinTests.Compare();
        //ArchiverTests.CreateBinMod(new Dictionary<string, string>());
        //GfxbinTests.Import();
    }
}