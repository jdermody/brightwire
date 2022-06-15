using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Helper
{
    /// <summary>
    /// Clones streams
    /// </summary>
    class StreamCloner : ICloneStreams, IDisposable
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

            public IStructByReferenceEnumerator<T> GetStructByReferenceEnumerator<T>(uint count) where T : struct
            {
                _stream.Seek(_position, SeekOrigin.Begin);
                return new ReferenceStructFromStreamReader<T>(_stream, count);
            }
            public IEnumerable<T> Enumerate<T>(uint count) where T : struct
            {
                _stream.Seek(_position, SeekOrigin.Begin);
                return _stream.Enumerate<T>(count);
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

            public IEnumerable<T> Enumerate<T>(uint count) where T : struct
            {
                _stream.Seek(_position, SeekOrigin.Begin);
                return _stream.Enumerate<T>(count);
            }

            public IStructByReferenceEnumerator<T> GetStructByReferenceEnumerator<T>(uint count) where T : struct
            {
                _stream.Seek(_position, SeekOrigin.Begin);
                return new ReferenceStructFromStreamReader<T>(_stream, count);
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
                _buffer = ms.GetBuffer();
            else
                throw new ArgumentException("Unsupported stream");
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public ICanReadSection Clone(long? position)
        {
            var pos = position ?? _stream.Position;
            if (_filePath != null)
                return new File(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), pos);
            return new Memory(new MemoryStream(_buffer!, false), pos);
        }
    }
}
