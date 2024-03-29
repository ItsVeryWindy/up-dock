﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDock.Imaging;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class DockerImage : IComparable<DockerImage>
    {
        private readonly List<object> _parts;

        public Uri Repository { get; }
        public string Image { get; }
        public string? Digest { get; }
        public string Tag => string.Join("", _parts.Select(x => x.ToString()));
        public DockerImageTemplate Template { get; }

        public IEnumerable<NuGetVersion> Versions => _parts.OfType<NuGetVersion>();

        public DockerImage(Uri repository, string image, string? digest, IEnumerable<object> parts, DockerImageTemplate template)
        {
            Repository = repository;
            Image = image;
            Digest = digest;
            Template = template;
            _parts = new List<object>(parts);
        }

        public bool CanUpgrade(DockerImage? other, bool allowDowngrade)
        {
            if (other is null)
                return false;

            if (Repository.Host != other.Repository.Host)
                return false;

            if (Image != other.Image)
                return false;

            if (Template != other.Template)
                return false;

            if (Template.HasDigest && Digest != other.Digest && _parts.Count != other._parts.Count)
                return true;

            if (allowDowngrade)
                return true;

            return other.CompareTo(this) > 0;
        }

        public int CompareTo(DockerImage? other)
        {
            if (other == null)
                return 1;

            if (Repository is null && other.Repository is not null)
                return -1;

            if (Repository is not null && other.Repository is null)
                return 1;

            if (Repository is not null && other.Repository is not null)
            {
                var repository = string.Compare(Repository.Host, other.Repository.Host,
                    StringComparison.InvariantCultureIgnoreCase);

                if (repository != 0)
                    return repository;
            }

            var image = string.Compare(Image, other.Image, StringComparison.InvariantCultureIgnoreCase);

            if (image != 0)
                return image;

            if (_parts.Count > other._parts.Count)
                return -1;

            if (_parts.Count < other._parts.Count)
                return 1;

            for (var i = 0; i < _parts.Count; i++)
            {
                if(_parts[i] is string strA && other._parts[i] is string strB)
                {
                    var compareStr = string.Compare(strA, strB, StringComparison.InvariantCultureIgnoreCase);

                    if (compareStr != 0)
                        return compareStr;
                }

                if(_parts[i] is NuGetVersion versionA && other._parts[i] is NuGetVersion versionB)
                {
                    var compareVersion = versionA.CompareTo(versionB);

                    if (compareVersion != 0)
                        return compareVersion;
                }

                if(_parts[i] is NuGetVersion && other._parts[i] is string)
                {
                    return -1;
                }

                if(_parts[i] is string && other._parts[i] is NuGetVersion)
                {
                    return 1;
                }
            }

            return 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Repository is not null)
            {
                sb.Append(Repository.Host).Append('/');
            }

            sb.Append(Image);

            if (Digest != null)
            {
                sb.Append('@').Append(Digest);
            }

            if(_parts.Count > 0)
            {
                sb.Append(':').Append(Tag);
            }

            return sb.ToString();
        }

        public override int GetHashCode() => HashCode.Combine(Repository, Image, Digest, Tag);

        public override bool Equals(object? obj)
        {
            if (obj is not DockerImage image)
                return false;

            return Equals(Repository, image.Repository) && Equals(Image, image.Image) && Equals(Digest, image.Digest) && Equals(Tag, image.Tag);
        }
    }
}
