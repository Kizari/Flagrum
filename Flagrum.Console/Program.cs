using System;
using System.IO;
using System.Linq;
using DirectXTexNet;
using Flagrum.Console.Utilities;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

using var context = new FlagrumDbContext(new SettingsService());

var items = FileFinder.GetUrisByString("data://menu/", "btex");
foreach (var item in items)
{
    var fileName = item.Split('/').Last().Replace(".btex", "");
    var data = context.GetFileByUri(item);
    var converter = new TextureConverter();
    var png = converter.BtexToPng(data);
    var originalFinalName = @$"C:\Modding\BTex\{fileName}";
    var finalName = originalFinalName;
    var i = 1;
    while (File.Exists(finalName + ".png"))
    {
        i++;
        finalName = originalFinalName + i;
    }
    
    File.WriteAllBytes(finalName + ".png", png);
}

//
// var converter = new TextureConverter();
// var png = converter.BtexToPng(data);
// File.WriteAllBytes(@$"C:\Modding\BTex\0.png", png);
// return;
//
// foreach (var format in Enum.GetValues<DXGI_FORMAT>())
// {
//     try
//     {
//         var dds = BtexConverter.BtexToDds(data, (uint)format);
//         File.WriteAllBytes(@$"C:\Modding\BTex\{format}.dds", dds);
//     }
//     catch
//     {
//         
//     }
// }