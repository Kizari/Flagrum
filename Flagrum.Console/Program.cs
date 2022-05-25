using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Console.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Ps4;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;

// using var context = new FlagrumDbContext(new SettingsService());
// var location =
//     @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\datas\CUSA01633-patch_115\CUSA01633-patch\patch\patch2\level\dlc_ex\feather\root_feather.earc";
// using var unpacker = new Unpacker(location);
// var item = unpacker.UnpackFileByQuery("swftitle01.btex", out _);
// File.WriteAllBytes(@"C:\Modding\Chocomog\Testing\swftitle01.btex", item);
// return;

var btex = Btex.FromData(File.ReadAllBytes(
    @"C:\Modding\Chocomog\Testing\swf.btex"));

btex.Bitmap.Save(@"C:\Modding\Chocomog\Testing\swf.png", ImageFormat.Png);
return;

var dds = btex.ToDds();

var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
var pointer = pinnedData.AddrOfPinnedObject();

var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);

pinnedData.Free();

using var stream = new MemoryStream();
using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
ddsStream.CopyTo(stream);
File.WriteAllBytes(
    @"C:\Modding\Chocomog\Scout\character\nh\nh24\model_000\sourceimages\nh24_000_skin_00_n.dds",
    stream.ToArray());

var x = true;