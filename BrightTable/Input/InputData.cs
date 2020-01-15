using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightTable.Input
{
    public class InputData : IDisposable
    {
        readonly MemoryStream _memoryStream, _baseStream;
        readonly FileStream _fileStream;
        readonly bool _isOwner;
 
        public InputData(MemoryStream memoryStream) : this(memoryStream, true)
        {
        }

        public InputData(string filePath) : this(new FileStream(filePath, FileMode.Open, FileAccess.Read), true)
        {
        }

        InputData(Stream stream, bool isOwner = false)
        {
            _isOwner = isOwner;
            if (stream is FileStream fileStream) {
                _fileStream = fileStream;
                Reader = new BinaryReader(_fileStream, Encoding.UTF8, true);
            } else if (stream is MemoryStream memoryStream) {
                _baseStream = _memoryStream = memoryStream;
                Reader = new BinaryReader(_memoryStream, Encoding.UTF8, true);
            }
        }

        InputData(MemoryStream baseStream, ArraySegment<byte> buffer)
        {
            _isOwner = false;
            _baseStream = baseStream;
            _memoryStream = new MemoryStream(buffer.Array, false);
            Reader = new BinaryReader(_memoryStream, Encoding.UTF8, true);
        }

        public void Dispose()
        {
            if (_isOwner) {
                _memoryStream?.Dispose();
                _fileStream?.Dispose();
            }
        }

        public BinaryReader Reader { get; }

        public long Position
        {
            get
            {
                if (_memoryStream != null)
                    return _memoryStream.Position;
                return _fileStream.Position;
            }
        }

        public void MoveTo(long offset)
        {
            if (_memoryStream != null)
                _memoryStream.Seek(offset, SeekOrigin.Begin);
            else
                _fileStream.Seek(offset, SeekOrigin.Begin);
        }

        public InputData Clone()
        {
            if (_baseStream != null && _baseStream.TryGetBuffer(out var buffer)) {
                return new InputData(_baseStream, buffer);
            } 

            if (_fileStream != null) {
                var stream = new FileStream(_fileStream.Name, FileMode.Open, FileAccess.Read);
                return new InputData(stream, true);
            }

            throw new Exception("Could not clone");
        }
    }
}
