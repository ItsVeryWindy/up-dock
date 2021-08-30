using System;
using System.Diagnostics.CodeAnalysis;
using static UpDock.Versioning.SemanticVersionUtilities;

namespace UpDock.Versioning
{
    public class Build
    {
        private readonly BuildIdentifier[] _identifiers;

        public static Build None { get; } = new(Array.Empty<BuildIdentifier>());

        private Build(BuildIdentifier[] identifiers)
        {
            _identifiers = identifiers;
        }

        public override string ToString() => _identifiers.Length > 0 ? $"+{string.Join('.', (object[])_identifiers)}" : "";

        public override bool Equals(object? obj)
        {
            if (obj is not Build build)
                return false;

            return this == build;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var identifier in _identifiers)
            {
                hashCode.Add(identifier);
            }

            return hashCode.ToHashCode();
        }

        public static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out Build build)
        {
            build = null;

            if (!TryParseIdentifiers<BuildIdentifier>(span, '.', BuildIdentifier.TryParse, out var identifiers))
                return false;

            build = new Build(identifiers);

            return true;
        }

        private class BuildIdentifier
        {
            private readonly string _identifier;

            public BuildIdentifier(string identifier)
            {
                _identifier = identifier;
            }

            public override string ToString() => _identifier;

            public override bool Equals(object? obj)
            {
                if (obj is not BuildIdentifier identifier)
                    return false;

                return this == identifier;
            }

            public override int GetHashCode() => _identifier.GetHashCode();

            public static bool TryParse(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out BuildIdentifier value)
            {
                value = null;

                if (!IsDigits(span) && !IsAlphaNumericIdentifier(span))
                    return false;

                value = new(span.ToString());

                return true;
            }

            public static bool operator ==(BuildIdentifier a, BuildIdentifier b)
            {
                return a._identifier == b._identifier;
            }

            public static bool operator !=(BuildIdentifier a, BuildIdentifier b)
            {
                return a._identifier != b._identifier;
            }
        }
    }
}
