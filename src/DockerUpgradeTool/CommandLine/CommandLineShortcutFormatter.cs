using System;
using System.Linq;
using System.Reflection;

namespace DockerUpgradeTool.CommandLine
{
    public static class Formatter
    {
        public static string FormatShortcut(PropertyInfo property) => string.Join('/', property.GetCustomAttributes<ShortcutAttribute>().SelectMany(x => x.Shortcuts).OrderByDescending(x => x, StringComparer.InvariantCultureIgnoreCase));
    }
}
