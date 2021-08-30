using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace UpDock.Versioning
{
    public static class SemanticVersionUtilities
    {
        public static bool IsNumericIdentifier(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return false;

            if (span.Length == 1 && IsDigit(span[0]))
                return true;

            if (!IsPositiveDigit(span[0]))
                return false;

            return IsDigits(span[1..]);
        }

        public static bool IsAlphaNumericIdentifier(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return false;

            if (IsNonDigit(span[0]))
            {
                if (span.Length == 1)
                    return true;

                return IsIdentifierCharacters(span[1..]);
            }

            if (IsNonDigit(span[^1]))
                return IsIdentifierCharacters(span[0..^1]);

            if (!IsIdentifierCharacter(span[0]))
                return false;

            var containsNonDigit = false;

            foreach (var chr in span[1..])
            {
                if (!containsNonDigit && IsNonDigit(chr))
                {
                    containsNonDigit = true;
                    continue;
                }

                if (!IsIdentifierCharacter(chr))
                    return false;
            }

            return containsNonDigit;
        }

        public static bool IsNonDigit(char chr) => IsLetter(chr) || chr == '-';

        public static bool IsLetter(char chr) => (chr >= 'A' && chr <= 'Z') || (chr >= 'a' && chr <= 'z');

        public static bool IsDigits(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return false;

            foreach (var chr in span)
            {
                if (!IsDigit(chr))
                    return false;
            }

            return true;
        }

        public static bool IsDigit(char chr) => chr >= '0' && chr <= '9';

        public static bool IsPositiveDigit(char chr) => chr >= '1' && chr <= '9';

        public static bool IsIdentifierCharacters(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return false;

            foreach (var chr in span)
            {
                if (!IsIdentifierCharacter(chr))
                    return false;
            }

            return true;
        }

        public static bool IsIdentifierCharacter(char chr) => IsDigit(chr) || IsNonDigit(chr);

        private static Counter Count(ReadOnlySpan<char> span, char value, Span<int> list)
        {
            var pos = 0;
            int[]? extra = null;

            while (true)
            {
                var index = span.IndexOf(value);

                if (index == -1)
                    break;

                if (pos == list.Length)
                {
                    var oldExtra = extra;
                    extra = ArrayPool<int>.Shared.Rent(list.Length * 2);
                    list.TryCopyTo(extra);
                    list = extra;
                    if (oldExtra is not null)
                    {
                        ArrayPool<int>.Shared.Return(oldExtra);
                    }
                }

                span = span[(index + 1)..];
                list[pos++] = index;
            }

            return new Counter(list.Slice(0, pos), extra);
        }

        public delegate bool TryParseFunc<T>(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out T value);

        public static bool TryParseIdentifiers<T>(ReadOnlySpan<char> span, char separator, TryParseFunc<T> tryParse, [MaybeNullWhen(false)] out T[] identifiers)
        {
            using var counter = Count(span, separator, stackalloc int[8]);

            var separators = counter.AsSpan();

            identifiers = null;
            T? identifier;

            if (separators.IsEmpty)
            {
                if (!tryParse(span, out identifier))
                    return false;

                identifiers = new[] { identifier };

                return true;
            }

            for (var i = 0; i < separators.Length; i++)
            {
                var separatedSpan = span.Slice(0, separators[i]);

                if (!tryParse(separatedSpan, out identifier))
                    return false;

                identifiers ??= new T[separators.Length + 1];

                identifiers[i] = identifier;

                span = span[(separators[i] + 1)..];
            }

            if (!tryParse(span, out identifier))
                return false;

            identifiers![^1] = identifier;

            return true;
        }

        public ref struct Counter
        {
            private readonly Span<int> _span;
            private readonly int[]? _pooledArray;

            public Counter(Span<int> span, int[]? pooledArray)
            {
                _span = span;
                _pooledArray = pooledArray;
            }

            public Span<int> AsSpan() => _span;

            public void Dispose()
            {
                if (_pooledArray is null)
                    return;

                ArrayPool<int>.Shared.Return(_pooledArray);
            }
        }
    }
}
