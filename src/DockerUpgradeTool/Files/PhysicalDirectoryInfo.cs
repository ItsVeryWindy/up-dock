using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DockerUpgradeTool.Files
{
    public class PhysicalDirectoryInfo : IDirectoryInfo
    {
        private readonly DirectoryInfo _directory;

        public PhysicalDirectoryInfo(DirectoryInfo directory)
        {
            _directory = directory;
        }

        public IEnumerable<IFileInfo> Files => _directory.GetFiles("*", SearchOption.AllDirectories).Select(x => new PhysicalFileInfo(x));

        public string AbsolutePath => _directory.FullName;

        public string Name => _directory.Name;

        public IDirectoryInfo? Parent => _directory.Parent == null ? null : new PhysicalDirectoryInfo(_directory.Parent);

        public IFileInfo GetFile(string relativePath) => new PhysicalFileInfo(new FileInfo(Path.Join(_directory.FullName, relativePath)));
    }
}
