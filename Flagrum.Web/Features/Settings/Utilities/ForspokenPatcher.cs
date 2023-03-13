// using System.IO;
// using Flagrum.Core.Utilities;
// using Flagrum.Web.Services;
//
// namespace Flagrum.Web.Features.Settings.Utilities;
//
// public class ForspokenPatcher
// {
//     private readonly ProfileService _profile;
//
//     public ForspokenPatcher(ProfileService profile)
//     {
//         _profile = profile;
//     }
//
//     private byte[] _signature => new byte[] {0x49, 0x8B, 0xD6, 0xFF, 0x50, 0x40, 0x4D, 0x3B, 0xFC, 0x0F};
//     private byte[] _signaturePatched => new byte[] {0x49, 0x8B, 0xD6, 0xFF, 0x50, 0x40, 0x4D, 0x39, 0xE4, 0x0F};
//
//     public void Patch()
//     {
//         var patch = new byte[] {0x39, 0xE4};
//         ApplyPatch(_signature, patch);
//     }
//
//     public void Unpatch()
//     {
//         var patch = new byte[] {0x3B, 0xFC};
//         ApplyPatch(_signaturePatched, patch);
//     }
//
//     private void ApplyPatch(byte[] signature, byte[] patch)
//     {
//         if (!File.Exists(_profile.GamePath.Replace(".exe", ".backup")))
//         {
//             File.Copy(_profile.GamePath, _profile.GamePath.Replace(".exe", ".backup"));
//         }
//         
//         var exe = File.ReadAllBytes(_profile.GamePath);
//         var finder = new BoyerMoore(signature);
//         var results = finder.Search(exe);
//
//         var patchOffset = -1;
//         foreach (var offset in results)
//         {
//             var postSignatureOffset = offset + 13;
//             var postSignatureBytes = exe[postSignatureOffset..(postSignatureOffset + 3)];
//             if (postSignatureBytes[0] == 0x00 && postSignatureBytes[1] == 0x00 && postSignatureBytes[2] == 0x48)
//             {
//                 patchOffset = offset + 7;
//                 break;
//             }
//         }
//         
//         using var stream = new FileStream(_profile.GamePath, FileMode.Open, FileAccess.Write);
//         stream.Seek(patchOffset, SeekOrigin.Begin);
//         stream.Write(patch);
//     }
// }