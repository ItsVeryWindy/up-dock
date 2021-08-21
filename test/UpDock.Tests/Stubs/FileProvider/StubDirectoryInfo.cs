using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UpDock.Files;

namespace UpDock.Tests
{
    internal class StubDirectoryInfo : IDirectoryInfo
    {
        private readonly Dictionary<string, StubStoredFile> _files;

        public IEnumerable<IFileInfo> AllFiles => _files.Where(x => x.Key.StartsWith(AbsolutePath)).Select(x => new StubFileInfo(_files, x.Key));
        public string AbsolutePath { get; }

        public string Name
        {
            get
            {
                var split = AbsolutePath.Split('/');

                return split.Last();
            }
        }

        public IDirectoryInfo? Parent
        {
            get
            {
                var index = AbsolutePath.LastIndexOf('/', AbsolutePath.Length - 2);

                if (index == -1)
                    return null;

                return new StubDirectoryInfo(_files, AbsolutePath.Substring(0, index));
            }
        }

        public bool Exists => _files.ContainsKey(AbsolutePath);

        public IEnumerable<IFileInfo> Files
        {
            get
            {
                var bits = AbsolutePath.Count(x => x == '/');

                return _files
                    .Keys
                    .Where(x => x.StartsWith(AbsolutePath))
                    .Where(x => x.Count(y => y == '/') == bits)
                    .Select(x => new StubFileInfo(_files, x));
            }
        }

        public IEnumerable<IDirectoryInfo> Directories
        {
            get
            {
                var bits = AbsolutePath.Count(x => x == '/') + 1;

                return _files
                    .Keys
                    .Where(x => x.EndsWith('/'))
                    .Where(x => x.StartsWith(AbsolutePath))
                    .Where(x => x.Count(y => y == '/') == bits)
                    .Select(x => new StubDirectoryInfo(_files, x));
            }
        }


        public StubDirectoryInfo(Dictionary<string, StubStoredFile> files, string path)
        {
            _files = files;
            AbsolutePath = path;
        }

        public void Delete()
        {
            var keys = _files.Keys.Where(x => x.StartsWith(AbsolutePath)).ToList();

            foreach (var key in keys)
            {
                _files.Remove(key);
            }
        }

        public IDirectoryInfo SetAttributes(FileAttributes attributes)
        {
            if (!_files.TryGetValue(AbsolutePath, out var value))
                return this;

            value.Attributes = attributes;

            return this;
        }

        public IFileInfo GetFile(string relativePath) => new StubFileInfo(_files, $"{AbsolutePath}{relativePath}");
        public IDirectoryInfo Create()
        {
            var index = 0;

            while ((index = AbsolutePath.IndexOf('/', index)) != -1)
            {
                var subPath = AbsolutePath.Substring(0, ++index);

                _files[subPath] = new StubStoredFile();
            }

            return this;
        }
    }
}
