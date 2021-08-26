using System;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public ref struct SearchTreeNodeContext
    {
        public SearchTreeNodeContext(ReadOnlySpan<char> span, int index, ReadOnlySpan<char> digest, ImmutableList<NuGetVersion> versions) : this()
        {
            Span = span;
            Index = index;
            Digest = digest;
            Versions = versions;
        }

        public ReadOnlySpan<char> Span { get; }
        public int Index { get; }
        public ReadOnlySpan<char> Digest { get; }
        public ImmutableList<NuGetVersion> Versions { get; }

        public SearchTreeNodeContext Next(int length) => new(Span[length..], Index + length, Digest, Versions);
        public SearchTreeNodeContext WithDigest(ReadOnlySpan<char> digest) => new(Span, Index, digest, Versions);
        public SearchTreeNodeContext WithVersion(NuGetVersion version) => new(Span, Index, Digest, Versions.Add(version));
    }
}
