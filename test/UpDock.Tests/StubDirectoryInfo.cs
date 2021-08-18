using System;
using System.Collections.Generic;
using System.IO;
using UpDock.Files;

namespace UpDock.Tests
{
    internal class StubDirectoryInfo : IDirectoryInfo
    {
        public IEnumerable<IFileInfo> AllFiles { get; }
        public string AbsolutePath { get; }
        public string Name { get; }
        public IDirectoryInfo? Parent { get; }

        public bool Exists => throw new NotImplementedException();

        public IEnumerable<IFileInfo> Files => throw new NotImplementedException();

        public IEnumerable<IDirectoryInfo> Directories => throw new NotImplementedException();

        public StubDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFile(string v) => throw new NotImplementedException();
        public void Delete() => throw new NotImplementedException();
        public void SetAttributes(FileAttributes normal) => throw new NotImplementedException();
    }
}
