using System.IO;

namespace UpDock.Tests
{
    public class StubStoredFile
    {
        public Stream? Stream { get; set; }
        public FileAttributes Attributes { get; set; }
    }
}
