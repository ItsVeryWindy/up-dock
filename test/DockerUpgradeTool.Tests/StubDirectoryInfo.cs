using System;
using System.Collections.Generic;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Tests
{
    internal class StubDirectoryInfo : IDirectoryInfo
    {
        public IEnumerable<IFileInfo> Files { get; }
        public string AbsolutePath { get; }
        public string Name { get; }
        public IDirectoryInfo? Parent { get; }

        public StubDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFile(string v) => throw new NotImplementedException();
    }
}
