using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Tests
{
    internal class StubFileProvider : IFileProvider
    {
        private readonly Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

        public void AddFile(string path, string contents) => AddFile(path, new MemoryStream(Encoding.UTF8.GetBytes(contents)));

        public void AddFile(string path, Stream stream) => _files[path] = stream;

        public IDirectoryInfo GetDirectory(string path) => new StubDirectoryInfo(path);

        public IFileInfo CreateTemporaryFile() => new StubFileInfo(_files, Guid.NewGuid().ToString());

        public StubFileInfo GetFile(string path) => new StubFileInfo(_files, path);

        IFileInfo IFileProvider.GetFile(string path) => GetFile(path);
    }
}
