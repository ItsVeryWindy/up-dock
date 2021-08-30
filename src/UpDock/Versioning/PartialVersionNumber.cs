using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UpDock.Versioning
{
    public class PartialVersionNumber
    {
        public PartialValue<int> Major { get; }

        public PartialValue<int> Minor { get; }

        public PartialValue<int> Patch { get; }

        public PartialValue<int> Revision { get; }

        public PartialValue<Build> Build { get; }

        public PartialValue<Prerelease> Prerelease { get; }

        //public VersionNumber MinimumVersion { get; }

        //public VersionNumber MaximumVersion { get; }

        public PartialVersionNumber(PartialValue<int> major, PartialValue<int> minor, PartialValue<int> patch, PartialValue<int> revision, PartialValue<Build> build, PartialValue<Prerelease> prerelease)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
            Build = build;
            Prerelease = prerelease;
            //MinimumVersion = new(Major ?? 0, Minor ?? 0, Patch ?? 0, Revision ?? 0, Build ?? Build.None, Prerelease ?? Prerelease.Minimum);
            //MaximumVersion = new(Major ?? int.MaxValue, Minor ?? int.MaxValue, Patch ?? int.MaxValue, Revision ?? int.MaxValue, Build ?? Build.None, Prerelease ?? Prerelease.None);
        }

    public static PartialVersionNumber Parse(string s) => TryParse(s, out var version) ? version! : throw new FormatException();

        public override string ToString()
        {
            var sb = new StringBuilder();

            if(!Major.IsUnspecified)
            {
                sb.Append(Major.ToString());
            }

            if (!Minor.IsUnspecified)
            {
                sb.Append($".{Minor}");
            }

            if (!Patch.IsUnspecified)
            {
                sb.Append($".{Patch}");
            }

            if (!Revision.IsUnspecified)
            {
                sb.Append($".{Revision}");
            }

            if (!Prerelease.IsUnspecified)
            {
                sb.Append($"-{Prerelease}");
            }

            if (!Build.IsUnspecified)
            {
                sb.Append($"+{Build}");
            }

            return sb.ToString();
        }

        private static string VersionToString(int? number) => number is null ? "*" : number.ToString()!;

        public override bool Equals(object? obj)
        {
            if (obj is not PartialVersionNumber version)
                return false;

            return this == version;
        }

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Revision, Build, Prerelease);

        public static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out PartialVersionNumber version)
        {
            version = null;

            if (!TryParseNumber(span, out var index, out var major, out var build, out var prerelease))
                return false;

            if (index == span.Length)
            {
                version = new PartialVersionNumber(major, PartialValue<int>.Unspecified, PartialValue<int>.Unspecified, PartialValue<int>.Unspecified, build, prerelease);

                return true;
            }

            var afterMajor = span[index..];

            if (!TryParseNumber(afterMajor, out index, out var minor, out build, out prerelease))
                return false;

            if (index == afterMajor.Length)
            {
                version = new PartialVersionNumber(major, minor, PartialValue<int>.Unspecified, PartialValue<int>.Unspecified, build, prerelease);

                return true;
            }

            var afterMinor = afterMajor[index..];

            if (!TryParseNumber(afterMinor, out index, out var patch, out build, out prerelease))
                return false;

            if (index == afterMinor.Length)
            {
                version = new PartialVersionNumber(major, minor, patch, PartialValue<int>.Unspecified, build, prerelease);

                return true;
            }

            var afterPatch = afterMinor[index..];

            if (!TryParseNumber(afterPatch, out index, out var revision, out build, out prerelease) || index != afterPatch.Length)
                return false;

            version = new PartialVersionNumber(major, minor, patch, revision, build, prerelease);

            return true;
        }

        private static bool TryParseNumber(ReadOnlySpan<char> span, out int index, [MaybeNullWhen(false)] out PartialValue<int> number, [MaybeNullWhen(false)] out PartialValue<Build> build, [MaybeNullWhen(false)] out PartialValue<Prerelease> prerelease)
        {
            number = null;
            index = span.IndexOfAny('.', '-', '+');
            build = PartialValue<Build>.Unspecified;
            prerelease = PartialValue<Prerelease>.Unspecified;

            if (index < 0)
            {
                index = span.Length;

                return TryParseNumber(span, out number);
            }

            if (!TryParseNumber(span[0..index], out number))
                return false;

            var chr = span[index];

            index++;

            if (chr == '.')
                return true;

            var last = span[index..];

            index = span.Length;

            if (chr == '-')
                return TryParsePrerelease(last, out prerelease, out build);

            if (chr == '+')
                return TryParseBuild(last, out build);

            return true;
        }

        private static bool TryParseNumber(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out PartialValue<int> number)
        {
            number = null;

            if (span.IsEmpty)
                return false;

            if (span.Length == 1 && (span[0] == 'x' || span[0] == 'X' || span[0] == '*'))
            {
                number = PartialValue<int>.Any;
                return true;
            }

            if (!int.TryParse(span, out var result))
                return false;

            number = new PartialValue<int>(result);

            return true;
        }

        private static bool TryParsePrerelease(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out PartialValue<Prerelease> prerelease, [MaybeNullWhen(false)] out PartialValue<Build> build)
        {
            prerelease = null;
            build = null;

            if (span.IsEmpty)
                return false;

            var index = span.IndexOf('+');

            if (index < 0)
                return TryParsePrerelease(span, out prerelease);

            if (!TryParsePrerelease(span[0..index], out prerelease))
                return false;

            return TryParseBuild(span[(index + 1)..], out build);
        }

        private static bool TryParsePrerelease(ReadOnlySpan<char> span, [MaybeNullWhen(false)]out PartialValue<Prerelease> prerelease)
        {
            prerelease = null;

            if (span.Length == 1 && span[0] == '*')
            {
                prerelease = PartialValue<Prerelease>.Any;
                return true;
            }

            if (!Versioning.Prerelease.TryParse(span, out var result))
                return false;

            prerelease = new(result);

            return true;
        }

        private static bool TryParseBuild(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out PartialValue<Build> build)
        {
            build = null;

            if (span.Length == 1 && (span[0] == '*'))
            {
                build = PartialValue<Build>.Any;
                return true;
            }

            if (!Versioning.Build.TryParse(span, out var result))
                return false;

            build = new(result);

            return true;
        }

        public static bool operator ==(PartialVersionNumber a, PartialVersionNumber b)
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

        public static bool operator !=(PartialVersionNumber a, PartialVersionNumber b)
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
    }

    public class PartialValue<T>
    {
        public static PartialValue<T> Unspecified { get; } = new(default!);
        public static PartialValue<T> Any { get; } = new(default!);

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue => this != Unspecified && this != Any;

        public bool IsUnspecified => this == Unspecified;

        public bool IsAny => this == Any;

        public T? Value { get; }

        public PartialValue(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            if (this == Unspecified)
                return "";

            if (this == Any)
                return "*";

            return Value!.ToString()!;
        }
    }
}
