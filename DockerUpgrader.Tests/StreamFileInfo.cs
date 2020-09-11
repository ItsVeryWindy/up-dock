using System;
using System.IO;
using DockerUpgrader.Files;

namespace DockerUpgrader.Tests
{
    public class StreamFileInfo : IFileInfo
    {
        private readonly Stream _stream;

        public StreamFileInfo(Stream stream)
        {
            _stream = stream;
        }

        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        public IDirectoryInfo? Parent { get; }
        public string Path => throw new NotImplementedException();
        public bool Exists { get; }
        public Stream CreateWriteStream()
        {
            throw new System.NotImplementedException();
        }

        public Stream CreateReadStream()
        {
            var ms = new MemoryStream();

            _stream.Position = 0;

            _stream.CopyTo(ms);

            ms.Position = 0;

            return ms;
        }

        public void Move(IFileInfo file)
        {
            throw new System.NotImplementedException();
        }

        public string MakeRelativePath(IDirectoryInfo directory)
        {
            throw new System.NotImplementedException();
        }
    }
}