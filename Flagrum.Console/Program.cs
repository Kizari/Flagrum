using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Core.Archive;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities;

var btex = Btex.FromData(File.ReadAllBytes(
    @"C:\Modding\Chocomog\Scout\character\nh\nh24\model_000\sourceimages\nh24_000_skin_00_n.btex"));

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