using System.Text;

namespace SQEX.Luminous.Core.IO
{
    public static class Path
    {
        /// <summary>
        /// Converts a relative path into an absolute path.
        /// </summary>
        /// <param name="baseFilePath">The parent path.</param>
        /// <param name="targetFilePath">The path.</param>
        /// <returns>The resolved path.</returns>
        public static string ResolveRelativePath(string baseFilePath, string targetFilePath)
        {
            return Path.Combine(baseFilePath, targetFilePath);
        }

        /// <summary>
        /// Combine two paths.
        /// </summary>
        /// <param name="path1">The first path.</param>
        /// <param name="path2">The second path.</param>
        /// <returns>The combined path.</returns>
        public static string Combine(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }

            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }

            if (IsPathRooted(path2))
            {
                return path2;
            }

            var result = new StringBuilder(path1);
            var lastChar = path1[path1.Length - 1];
            if (lastChar != '/' && lastChar != '\\')
            {
                result.Append('/');
            }

            result.Append(path2);
            return result.ToString();
        }

        /// <summary>
        /// Determines if a path is rooted.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>True if it is rooted, else false.</returns>
        public static bool IsPathRooted(string path)
        {
            if (path == null)
            {
                return false;
            }

            if (path[0] == 0)
            {
                return false;
            }

            if (path[0] == '\\')
            {
                return true;
            }

            if (path[0] == '/')
            {
                return true;
            }

            if (path[0] == '%')
            {
                return true;
            }

            if ((path[1] == ':' && path[2] == '\\') || path[2] == '/')
            {
                return true;
            }

            return false;
        }
    }
}
