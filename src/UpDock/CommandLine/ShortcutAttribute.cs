using System;
using System.Collections.Generic;
using System.Linq;

namespace UpDock.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ShortcutAttribute : Attribute
    {
        public IReadOnlyList<Shortcut> Shortcuts { get; }

        public ShortcutAttribute(params string[] shortcuts)
        {
            Shortcuts = shortcuts.Select(x => new Shortcut(x)).ToList();
        }
    }
}
