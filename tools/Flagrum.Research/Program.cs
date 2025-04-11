using System.IO;
using System.Linq;
using Flagrum.Research.Utilities;
using Flagrum.Core.Graphics.Materials;

// Example: Find a hex pattern in most files (including non-archived files) and log their URIs to the console
FileFinder.Create(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV")
    .WithBlacklist(TypeFlags.BlacklistStrict)
    .QueryHexPattern("6562 6200 0400 0000 0000 0000 7602 0000 1D00 0000 426C 6163 6B2E 456E 7469 7479")
    .IncludeLooseFiles()
    .FindAndLog();

// Example: Find all materials with a specific texture and write their URIs to a file
FileFinder.Create(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV")
    .WithWhitelist(TypeFlags.Material)
    .QueryCustom(file =>
    {
        var material = new GameMaterial();
        material.Read(file.GetReadableData());
        return material.Textures.Any(t => t.Uri == "data://some/texture.tif");
    })
    .FindAndExecute(file => File.AppendText(file.Uri));