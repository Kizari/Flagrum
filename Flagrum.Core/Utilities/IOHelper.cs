using System.IO;
using System.Reflection;

namespace Flagrum.Core.Utilities
{
    public static class IOHelper
    {
        public static string GetExecutingDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}