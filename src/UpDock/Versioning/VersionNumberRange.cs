using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UpDock.Versioning
{
    public class VersionNumberRange
    {
        private readonly IRange _range;

        private VersionNumberRange(IRange range)
        {
            _range = range;
        }

        public bool Satisfies(VersionNumber version) =>  _range.Satisfies(version);

        public override string ToString() => _range.ToString()!;

        public static VersionNumberRange Parse(ReadOnlySpan<char> span) => TryParse(span, out VersionNumberRange? range) ? range : throw new FormatException();

        public static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out VersionNumberRange rangeSet)
        {
            rangeSet = null;

            if (!TryParse(span, out IRange? range))
                return false;

            rangeSet = new VersionNumberRange(range);

            return true;
        }

        private static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            if (TryParseOr(span, out range))
                return true;

            return TryParseRange(span, out range);
        }

        private static bool TryParseOr(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            range = null;

            var index = span.IndexOf('|');

            if (index < 0)
                return false;

            if (span.Length < index + 1)
                return false;

            if (span[index + 1] != '|')
                return false;

            var firstSpace = FirstSpace(span, index);

            if (!TryParseRange(span[0..firstSpace], out var aRange))
                return false;

            var lastSpace = LastSpace(span[(index + 2)..]);

            if (!TryParse(span[(index + 2 + lastSpace)..], out IRange? bRange))
                return false;

            range = new OrRange(aRange, bRange);

            return true;
        }

        private static int FirstSpace(ReadOnlySpan<char> span, int start)
        {
            while(start > 1 && span[start - 1] == ' ')
            {
                start--;
            }

            return start;
        }

        private static int LastSpace(ReadOnlySpan<char> span)
        {
            var start = 0;

            while (start < span.Length && span[start] == ' ')
            {
                start++;
            }

            return start;
        }

        private static bool TryParseRange(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            if (TryParseHyphen(span, out range))
                return true;

            if (TryParseSimples(span, out range))
                return true;

            return false;
        }

        private static bool TryParseSimples(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            range = null;

            if (!SemanticVersionUtilities.TryParseIdentifiers<IRange>(span, ' ', TryParseSimple, out var ranges))
                return false;

            range = ranges.Length == 1 ? ranges[0] : new SimplesRange(ranges);

            return true;
        }

        private static bool TryParseSimple(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            if (TryParsePrimitive(span, out range))
                return true;

            if (PartialVersionNumber.TryParse(span, out var version))
            {
                range = new PartialRange(version);
                return true;
            }

            if (TryParseTilde(span, out range))
                return true;

            if (TryParseCaret(span, out range))
                return true;

            return false;
        }

        private static bool TryParseTilde(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            range = null;

            if (span.IsEmpty)
                return false;

            if (TryParsePrefixed(span, "~>", pv => new TildeRange(pv), out range))
                return true;

            return TryParsePrefixed(span, "~", pv => new TildeRange(pv), out range);
        }

        private static bool TryParseCaret(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range) => TryParsePrefixed(span, "^", pv => new CaretRange(pv), out range);

        private static bool TryParsePrefixed(ReadOnlySpan<char> span, ReadOnlySpan<char> prefix, Func<PartialVersionNumber, IRange> factory, [MaybeNullWhen(false)] out IRange range)
        {
            range = null;

            if (span.IsEmpty)
                return false;

            if (!span.StartsWith(prefix))
                return false;

            if (!PartialVersionNumber.TryParse(span[prefix.Length..], out var version))
                return false;

            range = factory(version);

            return true;
        }

        private static bool TryParsePrimitive(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            range = null;

            if (span.IsEmpty)
                return false;

            if (TryParsePrefixed(span, "=", vp => new EqualsRange(vp), out range))
                return true;

            if (TryParsePrefixed(span, ">=", vp => new GreaterThanOrEqualToRange(vp), out range))
                return true;

            if (TryParsePrefixed(span, "<=", vp => new LessThanOrEqualToRange(vp), out range))
                return true;

            if (TryParsePrefixed(span, ">", vp => new GreaterThanRange(vp), out range))
                return true;

            return TryParsePrefixed(span, "<", vp => new LessThanRange(vp), out range);
        }

        private static bool TryParseHyphen(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out IRange range)
        {
            range = null;

            var index = span.IndexOf(' ');

            if (index < 0)
                return false;

            if (span.Length < index + 3)
                return false;

            if (span[index + 1] != '-')
                return false;

            if (span[index + 2] != ' ')
                return false;

            if (!PartialVersionNumber.TryParse(span[0..index], out var a))
                return false;

            if (!PartialVersionNumber.TryParse(span[(index + 3)..], out var b))
                return false;

            range = new HyphenRange(a, b);

            return true;
        }

        private interface IRange
        {
            bool Satisfies(VersionNumber version);
        }

        private class OrRange : IRange
        {
            private readonly IRange _a;
            private readonly IRange _b;

            public OrRange(IRange a, IRange b)
            {
                _a = a;
                _b = b;
            }

            public bool Satisfies(VersionNumber version) => _a.Satisfies(version) || _b.Satisfies(version);

            public override string ToString() => $"{_a} || {_b}";
        }

        private class SimplesRange : IRange
        {
            private readonly IRange[] _ranges;

            public SimplesRange(IRange[] ranges)
            {
                _ranges = ranges;
            }

            public bool Satisfies(VersionNumber version) => _ranges.All(x => x.Satisfies(version));

            public override string ToString() => string.Join(' ', (object[])_ranges);
        }

        private class EqualsRange : PartialRange
        {
            private readonly PartialVersionNumber _partial;

            public EqualsRange(PartialVersionNumber partial) : base(partial)
            {
                _partial = partial;
            }

            public override string ToString() => $"={_partial}";
        }

        private class HyphenRange : IRange
        {
            private readonly PartialVersionNumber _a;
            private readonly PartialVersionNumber _b;
            private readonly VersionNumber _minimum;
            private readonly VersionNumber _maximum;

            public HyphenRange(PartialVersionNumber a, PartialVersionNumber b)
            {
                _a = a;
                _b = b;
                _minimum = CreateMinimum(a);
                _maximum = CreateMaximum(b);
            }

            private static VersionNumber CreateMinimum(PartialVersionNumber partial)
            {
                var major = CreateMinimumNumber(partial.Major);
                var minor = CreateMinimumNumber(partial.Minor);
                var patch = CreateMinimumNumber(partial.Patch);
                var revision = CreateMinimumNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreateMinimumPrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateMinimumNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreateMinimumPrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.Minimum;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            private static VersionNumber CreateMaximum(PartialVersionNumber partial)
            {
                var major = CreateMaximumNumber(partial.Major);
                var minor = CreateMaximumNumber(partial.Minor);
                var patch = CreateMaximumNumber(partial.Patch);
                var revision = CreateMaximumNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreateMaximumPrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateMaximumNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return int.MaxValue;

                if (number == PartialValue<int>.Any)
                    return int.MaxValue;

                return number.Value;
            }

            private static Prerelease CreateMaximumPrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.None;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.None;

                return prerelease.Value!;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_a.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                return version >= _minimum && version <= _maximum;
            }

            public override string ToString() => $"{_a} - {_b}";
        }

        private class PartialRange : IRange
        {
            private readonly PartialVersionNumber _partial;
            private readonly VersionNumber _minimum;
            private readonly VersionNumber _maximum;

            public PartialRange(PartialVersionNumber partial)
            {
                _partial = partial;
                _minimum = CreateMinimum(partial);
                _maximum = CreateMaximum(partial);
            }

            private static VersionNumber CreateMinimum(PartialVersionNumber partial)
            {
                var major = CreateMinimumNumber(partial.Major);
                var minor = CreateMinimumNumber(partial.Minor);
                var patch = CreateMinimumNumber(partial.Patch);
                var revision = CreateMinimumNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreateMinimumPrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateMinimumNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreateMinimumPrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.Minimum;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            private static VersionNumber CreateMaximum(PartialVersionNumber partial)
            {
                var major = CreateMaximumNumber(partial.Major);
                var minor = CreateMaximumNumber(partial.Minor);
                var patch = CreateMaximumNumber(partial.Patch);
                var revision = CreateMaximumNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreateMaximumPrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateMaximumNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return int.MaxValue;

                if (number == PartialValue<int>.Any)
                    return int.MaxValue;

                return number.Value;
            }

            private static Prerelease CreateMaximumPrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.None;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.None;

                return prerelease.Value!;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                return version >= _minimum && version <= _maximum;
            }

            public override string ToString() => _partial.ToString();
        }

        private class TildeRange : IRange
        {
            private readonly VersionNumber _minimum;
            private readonly VersionNumber _maximum;
            private readonly PartialVersionNumber _partial;

            public TildeRange(PartialVersionNumber partial)
            {
                _minimum = CreateMinimum(partial);
                _maximum = CreateMaximum(partial);
                _partial = partial;
            }

            private static VersionNumber CreateMinimum(PartialVersionNumber partial)
            {
                var major = CreateMinimumNumber(partial.Major);
                var minor = CreateMinimumNumber(partial.Minor);
                var patch = CreateMinimumNumber(partial.Patch);
                var revision = CreateMinimumNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreateMinimumPrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateMinimumNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreateMinimumPrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.None;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            private static VersionNumber CreateMaximum(PartialVersionNumber partial)
            {
                var major = CreateMaximumMajorNumber(partial);
                var minor = CreateMaximumMinorNumber(partial);

                return new(major, minor, 0, 0, Build.None, Prerelease.Minimum);
            }

            private static int CreateMaximumMajorNumber(PartialVersionNumber partial)
            {
                if (partial.Major == PartialValue<int>.Unspecified)
                    return int.MaxValue;

                if (partial.Major == PartialValue<int>.Any)
                    return int.MaxValue;

                if (partial.Minor == PartialValue<int>.Unspecified)
                    return partial.Major.Value + 1;

                return partial.Major.Value;
            }

            private static int CreateMaximumMinorNumber(PartialVersionNumber partial)
            {
                if (partial.Minor == PartialValue<int>.Unspecified)
                    return 0;

                if (partial.Minor == PartialValue<int>.Any)
                    return int.MaxValue;

                return partial.Minor.Value + 1;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                return version >= _minimum && version < _maximum;
            }

            public override string ToString() => $"~{_partial}";
        }

        private class CaretRange : IRange
        {
            private readonly VersionNumber _minimum;
            private readonly VersionNumber _maximum;
            private readonly PartialVersionNumber _partial;

            public CaretRange(PartialVersionNumber partial)
            {
                _minimum = CreateMinimum(partial);
                _maximum = CreateMaximum(partial);
                _partial = partial;
            }

            private static VersionNumber CreateMinimum(PartialVersionNumber partial)
            {
                var major = CreateMinimumNumber(partial.Major);
                var minor = CreateMinimumNumber(partial.Minor);
                var patch = CreateMinimumNumber(partial.Patch);
                var revision = CreateMinimumNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreateMinimumPrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateMinimumNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreateMinimumPrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.None;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            private static VersionNumber CreateMaximum(PartialVersionNumber partial)
            {
                var major = CreateMaximumMajorNumber(partial);
                var minor = CreateMaximumMinorNumber(partial);
                var patch = CreateMaximumPatchNumber(partial);
                var revision = CreateMaximumRevisionNumber(partial);

                return new(major, minor, patch, 0, Build.None, Prerelease.Minimum);
            }

            private static int CreateMaximumMajorNumber(PartialVersionNumber partial)
            {
                if (partial.Major == PartialValue<int>.Unspecified)
                    return 0;

                if (partial.Major == PartialValue<int>.Any)
                    return int.MaxValue;

                if (partial.Minor == PartialValue<int>.Unspecified || partial.Minor == PartialValue<int>.Any)
                    return partial.Major.Value + 1;

                return partial.Major.Value == 0 ? partial.Major.Value : partial.Major.Value + 1;
            }

            private static int CreateMaximumMinorNumber(PartialVersionNumber partial)
            {
                if (partial.Minor == PartialValue<int>.Unspecified)
                    return 0;

                if (partial.Minor == PartialValue<int>.Any)
                    return 0;

                if (partial.Minor.Value == 0)
                    return 0;

                return partial.Major.Value == 0 ? partial.Minor.Value + 1 : 0;
            }

            private static int CreateMaximumPatchNumber(PartialVersionNumber partial)
            {
                if (partial.Minor == PartialValue<int>.Unspecified)
                    return 0;

                if (partial.Minor == PartialValue<int>.Any)
                    return 0;

                return partial.Major.Value == 0 && partial.Minor.Value == 0 ? partial.Patch.Value + 1 : 0;
            }

            private static int CreateMaximumRevisionNumber(PartialVersionNumber partial)
            {
                if (partial.Patch == PartialValue<int>.Unspecified)
                    return 0;

                if (partial.Patch == PartialValue<int>.Any)
                    return 0;

                return partial.Major.Value == 0 && partial.Minor.Value == 0 && partial.Patch.Value == 0 ? partial.Revision.Value + 1 : 0;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                return version >= _minimum && version < _maximum;
            }
            public override string ToString() => $"^{_partial}";
        }

        private class GreaterThanRange : IRange
        {
            private readonly PartialVersionNumber _partial;
            private readonly VersionNumber _minimum;

            public GreaterThanRange(PartialVersionNumber partial)
            {
                _partial = partial;
                _minimum = CreateMinimum(partial);
            }

            private static VersionNumber CreateMinimum(PartialVersionNumber partial)
            {
                var major = CreateNumber(partial.Major, int.MaxValue);
                var minor = CreateNumber(partial.Minor, int.MaxValue);
                var patch = CreateNumber(partial.Patch, int.MaxValue);
                var revision = CreateNumber(partial.Revision, 0);
                var build = Build.None;
                var prerelease = CreatePrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateNumber(PartialValue<int> number, int unspecfied)
            {
                if (number == PartialValue<int>.Unspecified)
                    return unspecfied;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreatePrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.None;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                if(_partial.Prerelease.HasValue && version.Prerelease != Prerelease.None)
                {
                    if (_partial.Major.HasValue && _minimum.Major != version.Major)
                        return false;

                    if (_partial.Minor.HasValue && _minimum.Minor != version.Minor)
                        return false;

                    if (_partial.Patch.HasValue && _minimum.Patch != version.Patch)
                        return false;

                    if (_partial.Revision.HasValue && _minimum.Revision != version.Revision)
                        return false;
                }

                return version > _minimum;
            }

            public override string ToString() => $">{_partial}";
        }

        private class GreaterThanOrEqualToRange : IRange
        {
            private readonly PartialVersionNumber _partial;
            private readonly VersionNumber _minimum;

            public GreaterThanOrEqualToRange(PartialVersionNumber partial)
            {
                _partial = partial;
                _minimum = CreateMinimum(partial);
            }

            private static VersionNumber CreateMinimum(PartialVersionNumber partial)
            {
                var major = CreateNumber(partial.Major);
                var minor = CreateNumber(partial.Minor);
                var patch = CreateNumber(partial.Patch);
                var revision = CreateNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreatePrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreatePrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.Minimum;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                if (_partial.Prerelease.HasValue && version.Prerelease != Prerelease.None)
                {
                    if (_partial.Major.HasValue && _minimum.Major != version.Major)
                        return false;

                    if (_partial.Minor.HasValue && _minimum.Minor != version.Minor)
                        return false;

                    if (_partial.Patch.HasValue && _minimum.Patch != version.Patch)
                        return false;

                    if (_partial.Revision.HasValue && _minimum.Revision != version.Revision)
                        return false;
                }

                return version >= _minimum;
            }

            public override string ToString() => $">={_partial}";
        }

        private class LessThanRange : IRange
        {
            private readonly PartialVersionNumber _partial;
            private readonly VersionNumber _maximum;

            public LessThanRange(PartialVersionNumber partial)
            {
                _partial = partial;
                _maximum = CreateMaximum(partial);
            }

            private static VersionNumber CreateMaximum(PartialVersionNumber partial)
            {
                var major = CreateNumber(partial.Major);
                var minor = CreateNumber(partial.Minor);
                var patch = CreateNumber(partial.Patch);
                var revision = CreateNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreatePrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return 0;

                return number.Value;
            }

            private static Prerelease CreatePrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.Minimum;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.Minimum;

                return prerelease.Value!;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                return version < _maximum;
            }

            public override string ToString() => $"<{_partial}";
        }

        private class LessThanOrEqualToRange : IRange
        {
            private readonly PartialVersionNumber _partial;
            private readonly VersionNumber _maximum;

            public LessThanOrEqualToRange(PartialVersionNumber partial)
            {
                _partial = partial;
                _maximum = CreateMaximum(partial);
            }

            private static VersionNumber CreateMaximum(PartialVersionNumber partial)
            {
                var major = CreateNumber(partial.Major);
                var minor = CreateNumber(partial.Minor);
                var patch = CreateNumber(partial.Patch);
                var revision = CreateNumber(partial.Revision);
                var build = Build.None;
                var prerelease = CreatePrerelease(partial.Prerelease);

                return new(major, minor, patch, revision, build, prerelease);
            }

            private static int CreateNumber(PartialValue<int> number)
            {
                if (number == PartialValue<int>.Unspecified)
                    return 0;

                if (number == PartialValue<int>.Any)
                    return int.MaxValue;

                return number.Value;
            }

            private static Prerelease CreatePrerelease(PartialValue<Prerelease> prerelease)
            {
                if (prerelease == PartialValue<Prerelease>.Unspecified)
                    return Prerelease.None;

                if (prerelease == PartialValue<Prerelease>.Any)
                    return Prerelease.None;

                return prerelease.Value!;
            }

            public bool Satisfies(VersionNumber version)
            {
                if (_partial.Prerelease == PartialValue<Prerelease>.Unspecified && version.Prerelease != Prerelease.None)
                    return false;

                return version <= _maximum;
            }

            public override string ToString() => $"<={_partial}";
        }
    }
}
