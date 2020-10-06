using System;
using System.Collections.Generic;
using System.IO;
using DockerUpgradeTool.Files;
using DockerUpgradeTool.Git;

namespace DockerUpgradeTool.Tests
{
    internal class StubFileInfo : IRepositoryFileInfo, IFileInfo
    {
        private readonly Dictionary<string, Stream> _files;

        public StubFileInfo(Dictionary<string, Stream> files, string path)
        {
            _files = files;
            AbsolutePath = path;
        }

        public StubFileInfo(Stream stream, string path)
        {
            _files = new Dictionary<string, Stream>
            {
                [path] = stream
            };

            AbsolutePath = path;
        }

        public void Delete() => _files.Remove(AbsolutePath);

        public IDirectoryInfo? Parent { get; }
        public string AbsolutePath { get; }
        public string RelativePath => AbsolutePath;

        public bool Exists { get; }

        public IFileInfo File => this;

        public bool Ignored => throw new NotImplementedException();

        public IDirectoryInfo Root => throw new NotImplementedException();

        public Stream CreateWriteStream() => _files[AbsolutePath] = new StubMemoryStream();

        public Stream CreateReadStream()
        {
            var stream = _files[AbsolutePath];

            stream.Position = 0;

            var newStream = new StubMemoryStream();

            stream.CopyTo(newStream);

            newStream.Position = 0;

            return newStream;
        }

        public void Move(IFileInfo file)
        {
            var stream = _files[AbsolutePath];

            _files[file.AbsolutePath] = stream;

            _files.Remove(AbsolutePath);
        }
        
        private class StubMemoryStream : MemoryStream
        {
            protected override void Dispose(bool disposing)
            {
                Flush();
                Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
