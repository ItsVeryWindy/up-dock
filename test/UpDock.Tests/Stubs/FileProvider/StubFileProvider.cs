using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UpDock.Files;

namespace UpDock.Tests
{
    internal class StubFileProvider : IFileProvider
    {
        private readonly Dictionary<string, StubStoredFile> _files = new();

        public void AddFile(string path, string contents) => AddFile(path, new MemoryStream(Encoding.UTF8.GetBytes(contents)));

        public void AddFile(string path, Stream stream) => _files[path] = new StubStoredFile { Stream = stream };

        public IDirectoryInfo GetDirectory(string path)
        {
            if (!path.EndsWith('/'))
                path += '/';

            return new StubDirectoryInfo(_files, path);
        }

        public IFileInfo CreateTemporaryFile() => new StubFileInfo(_files, Guid.NewGuid().ToString());

        public StubFileInfo GetFile(string path) => new(_files, path);

        IFileInfo? IFileProvider.GetFile(string? path) => path is null ? null : GetFile(path);

        public IDirectoryInfo CreateDirectory(string path)
        {
            if (!path.EndsWith('/'))
            {
                path += '/';
            }

            int index;

            while ((index = path.IndexOf('/')) != -1)
            {
                var subPath = path.Substring(0, index + 1);

                _files[subPath] = new StubStoredFile();
            }

            return new StubDirectoryInfo(_files, path);
        }

        public IDirectoryInfo CreateTemporaryDirectory() => GetDirectory($"/{Guid.NewGuid()}").Create();
    }
}
