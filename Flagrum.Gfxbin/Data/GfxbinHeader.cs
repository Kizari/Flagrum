using System.Collections.Generic;

namespace Flagrum.Gfxbin.Data
{
    public class GfxbinHeader
    {
        public uint Version { get; set; }
        public List<DependencyPath> Dependencies { get; } = new List<DependencyPath>();
        public List<ulong> Hashes { get; } = new List<ulong>();
    }
}
