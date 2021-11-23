using Flagrum.Console.Utilities;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        CsvToJson.Parse("C:\\Testing\\Gfxbin\\Gmtl\\material_data.csv");
        //GfxbinTests.PrintData();
        //MaterialTests.BuildSkinMaterial();
        //GfxbinTests.Export();
        //GfxbinTests.Compare();
        //ArchiverTests.CreateBinMod(new Dictionary<string, string>());
        //GfxbinTests.Import();
    }
}