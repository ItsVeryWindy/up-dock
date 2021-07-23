using System.Collections.Generic;
using UpDock.Nodes;
using UpDock.Registry;

namespace UpDock.Caching
{
    /*
     * {
     *   "images": {
     *     "mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}": "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11"
     *   },
     *   "repositories": {
     *     "https://git/blah/blah": {
     *       "hash": "config-hash + sha",
     *       "entries": [0]
     *     }
     *   }
     * }
     */

    public class UpdateCacheEntry
    {
        private readonly IVersionCache _versionCache;
        public HashSet<DockerImage> Images { get; }
        public string Hash { get; private set; }

        public UpdateCacheEntry(IVersionCache versionCache, string hash)
        {
            _versionCache = versionCache;
            Hash = hash;
            Images = new HashSet<DockerImage>();
        }

        public bool HasChanged(string hash)
        {
            if (hash != Hash)
                return false;

            foreach(var image in Images)
            {
                var latest = _versionCache.FetchLatest(image.Template);

                if (latest is null)
                    continue;

                if (!image.Equals(latest))
                    return true;
            }

            return false;
        }
    }
}
