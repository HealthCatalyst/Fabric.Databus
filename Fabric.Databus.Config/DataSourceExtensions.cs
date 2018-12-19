using System;

namespace Fabric.Databus.Config
{
    using System.Linq;

    public static class DataSourceExtensions
    {
        public static int GetNestedLevel(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return path.Count(c => c == '.');
        }

        public static string GetPathOfParent(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (path == "$") return null;

            var lastIndexOf = path.LastIndexOf('.');
            var parentPropertyName = path.Substring(0, lastIndexOf);
            return parentPropertyName;
        }

        public static bool EntityNameEquals(this string name1, string name2)
        {
            if (name1 == null && name2 == null) return true;
            if ((name1 == null && name2 != null) || (name1 != null && name2 == null)) return false;

            var cleanedName1 = name1.Replace("[", string.Empty).Replace("]", string.Empty);
            var cleanedName2 = name2.Replace("[", string.Empty).Replace("]", string.Empty);

            return cleanedName1.Equals(cleanedName2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
