using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Tests
{
    internal class StubFileInfo : IFileInfo
    {
        private readonly Dictionary<string, StubStoredFile> _files;

        public StubFileInfo(Dictionary<string, StubStoredFile> files, string path)
        {
            _files = files;
            AbsolutePath = path;
        }

        public StubFileInfo(Stream stream, string path)
        {
            _files = new()
            {
                [path] = new StubStoredFile()
                {
                    Stream = stream
                }
            };

            AbsolutePath = path;
        }

        public void Delete() => _files.Remove(AbsolutePath);

        public IDirectoryInfo? Parent { get; }
        
        public string AbsolutePath { get; }
        
        public bool Exists => _files.ContainsKey(AbsolutePath);

        public IFileInfo File => this;

        public Stream CreateWriteStream()
        {
            if (!_files.TryGetValue(AbsolutePath, out var value))
            {
                value = new StubStoredFile();
                _files.Add(AbsolutePath, value);
            }
            
            value.Stream = new StubMemoryStream();

            return value.Stream;
        }

        public Stream? CreateReadStream()
        {
            if (!_files.TryGetValue(AbsolutePath, out var value) || value.Stream is null)
                return null;

            value.Stream.Position = 0;

            var newStream = new StubMemoryStream();

            value.Stream.CopyTo(newStream);

            newStream.Position = 0;

            return newStream;
        }

        public void Move(IFileInfo file)
        {
            var item = _files[AbsolutePath];

            _files[file.AbsolutePath] = item;

            _files.Remove(AbsolutePath);
        }

        public void SetAttributes(FileAttributes attributes)
        {
            if (!_files.TryGetValue(AbsolutePath, out var value))
                return;

            value.Attributes = attributes;
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
