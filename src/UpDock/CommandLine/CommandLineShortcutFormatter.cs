using System;
using System.Linq;
using System.Reflection;

namespace UpDock.CommandLine
{
    public static class Formatter
    {
        public static string FormatShortcut(PropertyInfo property) => string.Join('/', property.GetCustomAttributes<ShortcutAttribute>().SelectMany(x => x.Shortcuts).OrderByDescending(x => x.Length).ThenBy(x => x, StringComparer.InvariantCultureIgnoreCase));
    }
}
