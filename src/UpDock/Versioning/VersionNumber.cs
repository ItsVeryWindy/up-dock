using System;
using System.Diagnostics.CodeAnalysis;

namespace UpDock.Versioning
{
    public class VersionNumber
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public int Revision { get; }

        public Build Build { get; }

        public Prerelease Prerelease { get; }

        public VersionNumber(int major, int minor, int patch, int revision, Build build, Prerelease prerelease)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
            Build = build;
            Prerelease = prerelease;
        }

        public static VersionNumber Parse(string s) => TryParse(s, out var version) ? version! : throw new FormatException();

        public override string ToString() => $"{Major}.{Minor}.{Patch}.{Revision}{Build}{Prerelease}";

        public override bool Equals(object? obj)
        {
            if (obj is not VersionNumber version)
                return false;

            return this == version;
        }

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Revision, Build, Prerelease);

        public static bool TryParse(string s, [MaybeNullWhen(false)] out VersionNumber version)
        {
            version = null;

            var span = s.AsSpan();

            if (!TryParseNumber(span, out var index, out var major, out var build, out var prerelease))
                return false;

            if (index == span.Length)
            {
                version = new VersionNumber(major, 0, 0, 0, build, prerelease);
                
                return true;
            }

            var afterMajor = span[index..];

            if (!TryParseNumber(afterMajor, out index, out var minor, out build, out prerelease))
                return false;

            if (index == afterMajor.Length)
            {
                version = new VersionNumber(major, minor, 0, 0, build, prerelease);

                return true;
            }

            var afterMinor = afterMajor[index..];

            if (!TryParseNumber(afterMinor, out index, out var patch, out build, out prerelease))
                return false;

            if (index == afterMinor.Length)
            {
                version = new VersionNumber(major, minor, patch, 0, build, prerelease);

                return true;
            }

            var afterPatch = afterMinor[index..];

            if (!TryParseNumber(afterPatch, out index, out var revision, out build, out prerelease) || index != afterPatch.Length)
                return false;

            version = new VersionNumber(major, minor, patch, revision, build, prerelease);

            return true;
        }

        private static bool TryParseNumber(ReadOnlySpan<char> span, out int index, out int number, [MaybeNullWhen(false)] out Build build, [MaybeNullWhen(false)] out Prerelease prerelease)
        {
            index = span.IndexOfAny('.', '-', '+');
            build = Build.None;
            prerelease = Prerelease.None;

            if (index < 0)
            {
                index = span.Length;
                return int.TryParse(span, out number);
            }

            if (!int.TryParse(span.Slice(0, index), out number))
                return false;

            var chr = span[index];

            index++;

            if (chr == '.')
                return true;

            var last = span[index..];

            index = span.Length;

            if (chr == '+')
                return TryParseBuild(last, out build, out prerelease);

            return Prerelease.TryParse(last, out prerelease);
        }

        private static bool TryParseBuild(ReadOnlySpan<char> span, out Build? build, out Prerelease? prerelease)
        {
            var index = span.IndexOf('-');

            prerelease = Prerelease.None;

            if (index < 0)
            {
                return Build.TryParse(span, out build);
            }

            if (!Build.TryParse(span.Slice(0, index), out build))
                return false;

            return Prerelease.TryParse(span.Slice(index + 1), out prerelease);
        }

        public static bool operator ==(VersionNumber a, VersionNumber b)
        {
            if (a.Major != b.Major)
                return false;

            if (a.Minor != b.Minor)
                return false;

            if (a.Patch != b.Patch)
                return false;

            if (a.Revision != b.Revision)
                return false;

            if (a.Build != b.Build)
                return false;

            return a.Prerelease == b.Prerelease;
        }

        public static bool operator !=(VersionNumber a, VersionNumber b)
        {
            if (a.Major != b.Major)
                return true;

            if (a.Minor != b.Minor)
                return true;

            if (a.Patch != b.Patch)
                return true;

            if (a.Revision != b.Revision)
                return true;

            if (a.Build != b.Build)
                return true;

            return a.Prerelease != b.Prerelease;
        }

        public static bool operator <(VersionNumber a, VersionNumber b)
        {
            if (a.Major < b.Major)
                return true;

            if (a.Major > b.Major)
                return false;

            if (a.Minor < b.Minor)
                return true;

            if (a.Minor > b.Minor)
                return false;

            if (a.Patch < b.Patch)
                return true;

            if (a.Patch > b.Patch)
                return false;

            if (a.Revision < b.Revision)
                return true;

            if (a.Revision > b.Revision)
                return false;

            return a.Prerelease < b.Prerelease;
        }

        public static bool operator <=(VersionNumber a, VersionNumber b)
        {
            if (a.Major < b.Major)
                return true;

            if (a.Major == b.Major && a.Minor < b.Minor)
                return true;

            if (a.Major == b.Major && a.Minor == b.Minor && a.Patch < b.Patch)
                return true;

            if (a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch && a.Revision < b.Revision)
                return true;

            return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch && a.Revision == b.Revision && a.Prerelease <= b.Prerelease;
        }

        public static bool operator >=(VersionNumber a, VersionNumber b)
        {
            if (a.Major > b.Major)
                return true;

            if (a.Major < b.Major)
                return false;

            if (a.Minor > b.Minor)
                return true;

            if (a.Minor < b.Minor)
                return false;

            if (a.Patch > b.Patch)
                return true;

            if (a.Patch < b.Patch)
                return false;

            if (a.Revision > b.Revision)
                return true;

            if (a.Revision < b.Revision)
                return false;

            return a.Prerelease >= b.Prerelease;
        }

        public static bool operator >(VersionNumber a, VersionNumber b)
        {
            if (a.Major > b.Major)
                return true;

            if (a.Major < b.Major)
                return false;

            if (a.Minor > b.Minor)
                return true;

            if (a.Minor < b.Minor)
                return false;

            if (a.Patch > b.Patch)
                return true;

            if (a.Patch < b.Patch)
                return false;

            if (a.Revision > b.Revision)
                return true;

            if (a.Revision < b.Revision)
                return false;

            return a.Prerelease > b.Prerelease;
        }
    }
}
