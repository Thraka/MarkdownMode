using System.Reflection;

namespace System.Reflection
{
    internal static class Extensions
    {
        public static string GetLocation(this Assembly self)
        {
            if (self == null)
            {
                return null;
            }

            return System.IO.Path.GetDirectoryName(self.Location);
        }
    }
}

namespace System.IO
{
    internal static class Extensions
    {
        public static string ResolveRelativePath(this FileInfo self, string relativePath)
        {
            if (self == null || !self.Exists)
            {
                return null;
            }

            return self.Directory.ResolveRelativePath(relativePath);
        }

        public static string ResolveRelativePath(this DirectoryInfo self, string relativePath)
        {
            if (self == null)
            {
                return null;
            }

            var basePath = self;
            while (relativePath != null && relativePath.StartsWith(".."))
            {
                if (basePath.Parent == null)
                {
                    // Relative path can't be resolved
                    return null;
                }

                relativePath = relativePath.TrimStart('.').TrimStart('\\');
                basePath = basePath.Parent;
            }

            return Path.Combine(basePath.FullName, relativePath);
        }
    }
}
