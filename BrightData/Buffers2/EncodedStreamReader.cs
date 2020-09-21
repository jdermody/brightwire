using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace BrightData.Buffers2
{
    public static class EncodedStreamReader
    {
        public static ICanEnumerate<T> GetReader<T>(Stream stream, IBrightDataContext context)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8);
            var type = (BufferType)reader.ReadByte();

            if (type == BufferType.EncodedString)
                return (ICanEnumerate<T>)new StringDecoder(reader, stream);

            if (type == BufferType.EncodedStruct)
                return (ICanEnumerate<T>) Activator.CreateInstance(typeof(StructDecoder<>).MakeGenericType(typeof(T)), reader, stream);

            if(type == BufferType.Object)
                return (ICanEnumerate<T>)Activator.CreateInstance(typeof(ObjectReader<>).MakeGenericType(typeof(T)), context, reader, stream);

            if (type == BufferType.String)
                return (ICanEnumerate<T>) new StringReader(reader, stream);

            if(type == BufferType.Struct)
                return (ICanEnumerate<T>)Activator.CreateInstance(typeof(StructReader<>).MakeGenericType(typeof(T)), context, reader, stream);

            throw new NotImplementedException();
        }

        class RepeatableStreamReader
        {
            readonly Stream _stream;
            readonly long _startPos;

            public RepeatableStreamReader(Stream stream)
            {
                _stream = stream;
                _startPos = stream.Position;
            }

            public BinaryReader GetReader()
            {
                _stream.Seek(_startPos, SeekOrigin.Begin);
                return new BinaryReader(_stream, Encoding.UTF8);
            }

            public Stream GetStream()
            {
                _stream.Seek(_startPos, SeekOrigin.Begin);
                return _stream;
            }
        }

        class StringDecoder : ICanEnumerate<string>
        {
            readonly RepeatableStreamReader _stream;
            readonly uint _length;
            readonly string[] _stringTable;

            public StringDecoder(BinaryReader reader, Stream stream)
            {
                var len = reader.ReadUInt32();
                _stringTable = new string[len];
                for (uint i = 0; i < len; i++)
                    _stringTable[i] = reader.ReadString();

                _length = reader.ReadUInt32();
                _stream = new RepeatableStreamReader(stream);
            }

            public IEnumerable<string> EnumerateTyped()
            {
                var reader = _stream.GetReader();
                for (uint i = 0; i < _length; i++)
                    yield return _stringTable[reader.ReadUInt32()];
            }
        }

        class StructDecoder<T> : ICanEnumerate<T>
            where T : struct
        {
            readonly RepeatableStreamReader _stream;
            readonly uint _length;
            readonly T[] _table;

            public StructDecoder(BinaryReader reader, Stream stream)
            {
                var len = reader.ReadUInt32();
                _table = new T[len];
                stream.Read(MemoryMarshal.Cast<T, byte>(_table));
                _length = reader.ReadUInt32();
                _stream = new RepeatableStreamReader(stream);
            }

            public IEnumerable<T> EnumerateTyped()
            {
                var reader = _stream.GetReader();
                for (uint i = 0; i < _length; i++)
                    yield return _table[reader.ReadUInt32()];
            }
        }

        class ObjectReader<T> : ICanEnumerate<T>
            where T : ICanInitializeFromBinaryReader
        {
            private readonly BrightDataContext _context;
            readonly RepeatableStreamReader _stream;
            private readonly uint _length;

            public ObjectReader(BrightDataContext context, BinaryReader reader, Stream stream)
            {
                _context = context;
                _length = reader.ReadUInt32();
                _stream = new RepeatableStreamReader(stream);
            }

            public IEnumerable<T> EnumerateTyped()
            {
                var reader = _stream.GetReader();
                for (uint i = 0; i < _length; i++) {
                    var ret = (T)FormatterServices.GetUninitializedObject(typeof(T));
                    ret.Initialize(_context, reader);
                    yield return ret;
                }
            }
        }

        class StringReader : ICanEnumerate<string>
        {
            readonly RepeatableStreamReader _stream;
            private readonly uint _length;

            public StringReader(BinaryReader reader, Stream stream)
            {
                _length = reader.ReadUInt32();
                _stream = new RepeatableStreamReader(stream);
            }

            public IEnumerable<string> EnumerateTyped()
            {
                var reader = _stream.GetReader();
                for (uint i = 0; i < _length; i++)
                    yield return reader.ReadString();
            }
        }

        class StructReader<T> : ICanEnumerate<T>
            where T : struct
        {
            readonly RepeatableStreamReader _stream;
            readonly uint _length;

            public StructReader(BinaryReader reader, Stream stream)
            {
                _length = reader.ReadUInt32();
                _stream = new RepeatableStreamReader(stream);
            }


            public IEnumerable<T> EnumerateTyped() => BufferedRead<T>(_stream.GetStream(), _length);
        }

        static IEnumerable<T> BufferedRead<T>(Stream stream, uint count, uint tempBufferSize = 8096)
            where T : struct
        {
            // simple case
            if (count < tempBufferSize) {
                var buffer = new T[count];
                stream.Read(MemoryMarshal.Cast<T, byte>(buffer));
                foreach (var item in buffer)
                    yield return item;
            }

            // buffered read
            var temp = new T[tempBufferSize];
            var sizeofT = MemoryMarshal.Cast<T, byte>(temp).Length / (int)tempBufferSize;
            var totalRead = 0;

            while (totalRead < count) {
                var readCount = stream.Read(MemoryMarshal.Cast<T, byte>(temp)) / sizeofT;
                for (var i = 0; i < readCount; i++)
                    yield return temp[i];
                totalRead += readCount;
            }
        }
    }
}
