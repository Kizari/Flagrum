using Flagrum.Console.Utilities;

// var packer = new Ps4EnvironmentPacker(new ConsoleLogger("Logger", () => new ConsoleLoggerConfiguration()),
//     new SettingsService());
// packer.Pack("data://level/dlc_ex/mog/area_ravettrice_mog.ebex",
//     @"C:\Modding\Chocomog\Testing\Environment\area_ravettrice_mog.fed");

await ChocomogPorter.Run();

// FileFinder.FindUriByString("al_pr_mogcho_gameGate.gmdl");
// return;
//
// using var context = new FlagrumDbContext(new SettingsService());
// var location =
//     @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\datas\CUSA01633-patch_115\CUSA01633-patch\patch\patch2_initial\common\first_packages_all.earc";
// using var unpacker = new Unpacker(location);
// var item = unpacker.UnpackFileByQuery("menuswfentity_title_special.ebex", out _);
// File.WriteAllBytes(@"C:\Modding\Chocomog\Testing\menuswfentity_title_special.ebex", item);
// return;
//
// var btex = Btex.FromData(item);
// btex.Bitmap.Save(@"C:\Modding\Chocomog\Testing\swf_mogchoco.png", ImageFormat.Png);
// return;
//
// var dds = btex.ToDds();
//
// var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
// var pointer = pinnedData.AddrOfPinnedObject();
//
// var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);
//
// pinnedData.Free();
//
// using var stream = new MemoryStream();
// using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
// ddsStream.CopyTo(stream);
// File.WriteAllBytes(
//     @"C:\Modding\Chocomog\Scout\character\nh\nh24\model_000\sourceimages\nh24_000_skin_00_n.dds",
//     stream.ToArray());
//
// var x = true;