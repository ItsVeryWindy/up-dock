using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DockerUpgrader.Files;

namespace DockerUpgrader.Tests
{
    class StubFileProvider : IFileProvider
    {
        Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

        public void AddFile(string path, Stream stream)
        {
            _files[path] = stream;
        }

        public IDirectoryInfo GetDirectory(string path)
        {
            return new StubDirectoryInfo(path);
        }

        public IFileInfo CreateTemporaryFile()
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFile(string path)
        {
            return new StubFileInfo(_files, path);
        }
    }

    internal class StubFileInfo : IFileInfo
    {
        private readonly Dictionary<string, Stream> _files;
        private readonly string _path;

        public StubFileInfo(Dictionary<string, Stream> files, string path)
        {
            _files = files;
            _path = path;
        }

        public void Delete()
        {
            _files.Remove(_path);
        }

        public IDirectoryInfo? Parent { get; }
        public string Name { get; }
        public string Path { get; }
        public bool Exists { get; }
        public Stream CreateWriteStream() => _files[_path] = new MemoryStream();

        public Stream CreateReadStream() => _files[_path];

        public void Move(IFileInfo file)
        {
            throw new NotImplementedException();
        }

        public string MakeRelativePath(IDirectoryInfo directory)
        {
            throw new NotImplementedException();
        }
    }

    internal class StubDirectoryInfo : IDirectoryInfo
    {
        public IEnumerable<IFileInfo> Files { get; }
        public string Path { get; }
        public string Name { get; }
        public IDirectoryInfo? Parent { get; }

        public StubDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }
    }
}
