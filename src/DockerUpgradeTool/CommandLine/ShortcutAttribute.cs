using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerUpgradeTool.CommandLine
{
    public class ShortcutAttribute : Attribute
    {
        public IReadOnlyList<string> Shortcuts { get; }

        public ShortcutAttribute(params string[] shortcuts)
        {
            Shortcuts = shortcuts.ToList();
        }
    }
}
