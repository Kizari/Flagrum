using System.Collections;
using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Data
{
    public class MeshBuffer
    {
        public byte[] Buffer { get; set; }
        public Dictionary<string, IList> Data { get; set; }
        public int[,] Faces { get; set; }
    }
}