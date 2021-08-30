using System;
using System.Diagnostics.CodeAnalysis;
using static UpDock.Versioning.SemanticVersionUtilities;

namespace UpDock.Versioning
{
    public class Prerelease
    {
        private readonly PrereleaseIdentifier[] _identifiers;

        public static Prerelease None { get; } = new(Array.Empty<PrereleaseIdentifier>());
        public static Prerelease Minimum { get; } = new(new[] { new PrereleaseIdentifier(0) });

        private Prerelease(PrereleaseIdentifier[] identifiers)
        {
            _identifiers = identifiers;
        }

        public override string ToString() => _identifiers.Length > 0 ? $"-{string.Join('.', (object[])_identifiers)}" : "";

        public override bool Equals(object? obj)
        {
            if (obj is not Prerelease prerelease)
                return false;

            return this == prerelease;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach(var identifier in _identifiers)
            {
                hashCode.Add(identifier);
            }

            return hashCode.ToHashCode();
        }

        public static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out Prerelease prerelease)
        {
            prerelease = null;

            if (!TryParseIdentifiers<PrereleaseIdentifier>(span, '.', PrereleaseIdentifier.TryParse, out var identifiers))
                return false;

            prerelease = new(identifiers);

            return true;
        }

        public static bool operator ==(Prerelease? a, Prerelease? b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null && b is not null)
                return false;

            if (a is not null && b is null)
                return false;

            if (a!._identifiers.Length != b!._identifiers.Length)
                return false;

            for (var i = 0; i < a._identifiers.Length; i++)
            {
                if (a._identifiers[i] != b._identifiers[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(Prerelease? a, Prerelease? b)
        {
            if (ReferenceEquals(a, b))
                return false;

            if (a is null && b is not null)
                return true;

            if (a is not null && b is null)
                return true;

            if (a!._identifiers.Length != b!._identifiers.Length)
                return true;

            for (var i = 0; i < a._identifiers.Length; i++)
            {
                if (a._identifiers[i] != b._identifiers[i])
                    return true;
            }

            return false;
        }

        public static bool operator <(Prerelease a, Prerelease b)
        {
            if (a._identifiers.Length == 0)
                return false;

            if (b._identifiers.Length == 0)
                return true;

            var minIdentifiers = Math.Min(a._identifiers.Length, b._identifiers.Length);

            for(var i = 0; i < minIdentifiers; i++)
            {
                if (a._identifiers[i] == b._identifiers[i])
                    continue;

                return a._identifiers[i] < b._identifiers[i];
            }

            return a._identifiers.Length < b._identifiers.Length;
        }

        public static bool operator <=(Prerelease a, Prerelease b)
        {
            if (a._identifiers.Length == 0 && b._identifiers.Length != 0)
                return false;

            if (b._identifiers.Length == 0)
                return true;

            var minIdentifiers = Math.Min(a._identifiers.Length, b._identifiers.Length);

            for (var i = 0; i < minIdentifiers; i++)
            {
                if (a._identifiers[i] == b._identifiers[i])
                    continue;

                return a._identifiers[i] < b._identifiers[i];
            }

            return a._identifiers.Length <= b._identifiers.Length;
        }

        public static bool operator >=(Prerelease a, Prerelease b)
        {
            if (a._identifiers.Length == 0)
                return b._identifiers.Length >= 0;

            if (b._identifiers.Length == 0)
                return false;

            var minIdentifiers = Math.Min(a._identifiers.Length, b._identifiers.Length);

            for (var i = 0; i < minIdentifiers; i++)
            {
                if (a._identifiers[i] == b._identifiers[i])
                    continue;

                return a._identifiers[i] > b._identifiers[i];
            }

            return a._identifiers.Length >= b._identifiers.Length;
        }

        public static bool operator >(Prerelease a, Prerelease b)
        {
            if (a._identifiers.Length == 0)
                return b._identifiers.Length > 0;

            if (b._identifiers.Length == 0)
                return false;

            var minIdentifiers = Math.Min(a._identifiers.Length, b._identifiers.Length);

            for (var i = 0; i < minIdentifiers; i++)
            {
                if (a._identifiers[i] == b._identifiers[i])
                    continue;

                return a._identifiers[i] > b._identifiers[i];
            }

            return a._identifiers.Length > b._identifiers.Length;
        }

        private class PrereleaseIdentifier
        {
            private readonly int? _number;

            private readonly string? _nonNumber;

            public PrereleaseIdentifier(int number)
            {
                _number = number;
            }

            public PrereleaseIdentifier(string nonNumber)
            {
                _nonNumber = nonNumber;
            }

            private bool IsNumeric => _number is not null;

            public override string ToString() => IsNumeric ? _number.ToString()! : _nonNumber!;

            public override bool Equals(object? obj)
            {
                if (obj is not PrereleaseIdentifier identifier)
                    return false;

                return this == identifier;
            }

            public override int GetHashCode() => HashCode.Combine(_number, _nonNumber);

            public static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out PrereleaseIdentifier identifier)
            {
                identifier = null;

                if(IsNumericIdentifier(span))
                {
                    if (!int.TryParse(span, out var result))
                        return false;

                    identifier = new(result);

                    return true;
                }

                if (!IsAlphaNumericIdentifier(span))
                    return false;

                identifier = new(span.ToString());

                return true;
            }

            public static bool operator ==(PrereleaseIdentifier a, PrereleaseIdentifier b)
            {
                return a._number == b._number && a._nonNumber == b._nonNumber;
            }

            public static bool operator !=(PrereleaseIdentifier a, PrereleaseIdentifier b)
            {
                return a._number != b._number || a._nonNumber != b._nonNumber;
            }

            public static bool operator <(PrereleaseIdentifier a, PrereleaseIdentifier b)
            {
                if (a.IsNumeric && !b.IsNumeric)
                    return true;

                if (!a.IsNumeric && b.IsNumeric)
                    return false;

                if (a.IsNumeric && b.IsNumeric)
                    return a._number < b._number;

                return string.Compare(a._nonNumber, b._nonNumber) < 0;
            }

            public static bool operator >(PrereleaseIdentifier a, PrereleaseIdentifier b)
            {
                if (a.IsNumeric && !b.IsNumeric)
                    return false;

                if (!a.IsNumeric && b.IsNumeric)
                    return true;

                if (a.IsNumeric && b.IsNumeric)
                    return a._number > b._number;

                return string.Compare(a._nonNumber, b._nonNumber) > 0;
            }
        }
    }
}
