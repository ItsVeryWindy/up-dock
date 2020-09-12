using System;
using System.Collections.Generic;
using System.IO;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Tests
{
    internal class StubFileProvider : IFileProvider
    {
        private readonly Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

        public void AddFile(string path, Stream stream) => _files[path] = stream;

        public IDirectoryInfo GetDirectory(string path) => new StubDirectoryInfo(path);

        public IFileInfo CreateTemporaryFile() => throw new NotImplementedException();

        public IFileInfo GetFile(string path) => new StubFileInfo(_files, path);
    }
}
