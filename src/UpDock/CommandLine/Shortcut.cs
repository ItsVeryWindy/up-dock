using System;
using System.Text.RegularExpressions;

namespace UpDock.CommandLine
{
    public class Shortcut
    {
        private readonly string _shortcut;
        private static readonly Regex Regex = new("^[a-z-]+$", RegexOptions.Compiled);

        public Shortcut(string shortcut)
        {
            if (shortcut is null)
                throw new ArgumentNullException(nameof(shortcut));

            if (shortcut.Length == 0)
                throw new ArgumentException("shortcut must have a length greater than zero", nameof(shortcut));

            if (!Regex.IsMatch(shortcut))
                throw new ArgumentException("shortcut can only contain dashes and alphabetical characters", nameof(shortcut));

            _shortcut = shortcut;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool stdin) => $"{(_shortcut.Length == 1 ? '-' : "--")}{(stdin ? '@' : "")}{_shortcut}";
    }
}
