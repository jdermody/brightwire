using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualBasic;

namespace BrightData.Helper
{
    /// <summary>
    /// Clones streams
    /// </summary>
    class StreamCloner : ICloneStreams
    {
        class File : ICanReadSection
        {
            readonly FileStream _stream;
            readonly long _position;

            public File(FileStream stream, long position)
            {
                _position = position;
                _stream = stream;
            }

            public BinaryReader GetReader()
            {
                _stream.Seek(_position, SeekOrigin.Begin);
                return new BinaryReader(_stream, Encoding.UTF8, true);
            }
            public void Dispose()
            {
                _stream.Dispose();
            }
        }

        class Memory : ICanReadSection
        {
            readonly MemoryStream _stream;
            readonly long _position;

            public Memory(MemoryStream stream, long position)
            {
                _stream = stream;
                _position = position;
            }

            public void Dispose()
            {
                _stream.Dispose();
            }

            public BinaryReader GetReader()
            {
                _stream.Seek(_position, SeekOrigin.Begin);
                return new BinaryReader(_stream, Encoding.UTF8, true);
            }
        }

        readonly string? _filePath;
        readonly byte[]? _buffer;
        readonly Stream _stream;

        public StreamCloner(Stream stream)
        {
            _stream = stream;
            if (stream is FileStream fs)
                _filePath = fs.Name;
            else if (stream is MemoryStream ms)
                _buffer = ms.ToArray();
            else
                throw new ArgumentException("Unsupported stream");
        }

        public ICanReadSection Clone()
        {
            if (_filePath != null)
                return new File(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), _stream.Position);
            return new Memory(new MemoryStream(_buffer!), _stream.Position);
        }
    }
}
