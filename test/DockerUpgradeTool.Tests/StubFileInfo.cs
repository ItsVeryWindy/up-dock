using System;
using System.Collections.Generic;
using System.IO;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Tests
{
    internal class StubFileInfo : IFileInfo
    {
        private readonly Dictionary<string, Stream> _files;

        public StubFileInfo(Dictionary<string, Stream> files, string path)
        {
            _files = files;
            Path = path;
        }

        public void Delete() => _files.Remove(Path);

        public IDirectoryInfo? Parent { get; }
        public IDirectoryInfo? Root { get; }
        public string Path { get; }
        public bool Exists { get; }
        public Stream CreateWriteStream() => _files[Path] = new MemoryStream();

        public Stream CreateReadStream() => _files[Path];

        public void Move(IFileInfo file) => throw new NotImplementedException();

        public string MakeRelativePath(IDirectoryInfo directory) => throw new NotImplementedException();
    }
}
