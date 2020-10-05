using System;
using System.IO;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Tests
{
    public class StreamFileInfo : IFileInfo
    {
        private readonly Stream _stream;

        public StreamFileInfo(Stream stream)
        {
            _stream = stream;
        }

        public void Delete() => throw new NotImplementedException();

        public IDirectoryInfo? Parent { get; }

        public IDirectoryInfo? Root { get; }

        public string Path => throw new NotImplementedException();
        public bool Exists { get; }
        public Stream CreateWriteStream() => throw new NotImplementedException();

        public Stream CreateReadStream()
        {
            var ms = new MemoryStream();

            _stream.Position = 0;

            _stream.CopyTo(ms);

            ms.Position = 0;

            return ms;
        }

        public void Move(IFileInfo file) => throw new NotImplementedException();

        public string MakeRelativePath(IDirectoryInfo directory) => throw new NotImplementedException();
    }
}
