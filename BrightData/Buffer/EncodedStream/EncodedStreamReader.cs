using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.Serialisation;

namespace BrightData.Buffer.EncodedStream
{
    /// <summary>
    /// Reads from potentially encoded storage
    /// </summary>
    internal static class EncodedStreamReader
    {
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

        class ReadFromRepeatableStream<T> : ICanEnumerateWithSize<T>
            where T : struct
        {
            readonly RepeatableStreamReader _stream;

            public ReadFromRepeatableStream(uint length, Stream stream)
            {
                Size = length;
                _stream = new RepeatableStreamReader(stream);
            }

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values => _stream.GetStream().Enumerate<T>(Size);
            public uint Size { get; }
        }

        class ConvertFromStream<T> : ICanEnumerateWithSize<T> where T : notnull
        {
            readonly Func<BinaryReader, T> _converter;
            readonly RepeatableStreamReader _stream;

            public ConvertFromStream(uint length, Stream stream, Func<BinaryReader, T> converter)
            {
                Size = length;
                _converter = converter;
                _stream = new RepeatableStreamReader(stream);
            }

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values
            {
                get
                {
                    var reader = _stream.GetReader();
                    for (uint i = 0; i < Size; i++)
                        yield return _converter(reader);
                }
            }

            public uint Size { get; }
        }

        class ReadFromMemory<T> : ICanEnumerateWithSize<T>
            where T : struct
        {
            readonly T[] _data;

            public ReadFromMemory(uint length, Stream stream)
            {
                _data = stream.Enumerate<T>(length).ToArray();
#if DEBUG
                Debug.Assert(_data.Length == length);
#endif
            }

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values => _data;
            public uint Size => (uint)_data.Length;
        }

        class LoadIntoMemory<T> : ICanEnumerateWithSize<T> where T : notnull
        {
            readonly T[] _data;

            public LoadIntoMemory(uint length, BinaryReader reader, Func<BinaryReader, T> objectBuilder)
            {
                _data = new T[length];
                for (uint i = 0; i < length; i++)
                    _data[i] = objectBuilder(reader);
            }

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values => _data;
            public uint Size => (uint)_data.Length;
        }

        static ICanEnumerateWithSize<T> GetReader<T>(uint length, uint inMemorySize, Stream stream)
            where T : struct
        {
            if (length <= inMemorySize)
                return new ReadFromMemory<T>(length, stream);
            return new ReadFromRepeatableStream<T>(length, stream);
        }

        static ICanEnumerateWithSize<T> GetReader<T>(uint length, uint inMemorySize, BinaryReader reader, Stream stream, Func<BinaryReader, T> objectBuilder) where T : notnull
        {
            if (length <= inMemorySize)
                return new LoadIntoMemory<T>(length, reader, objectBuilder);
            return new ConvertFromStream<T>(length, stream, objectBuilder);
        }

        public class StringDecoder : ICanEnumerateWithSize<string>, ICanWriteToBinaryWriter, IHaveStringTable
        {
            readonly ICanEnumerateWithSize<ushort> _reader;
            readonly string[] _stringTable;

            public StringDecoder(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                var len = reader.ReadUInt32();
                _stringTable = new string[len];
                for (uint i = 0; i < len; i++)
                    _stringTable[i] = reader.ReadString();
                var indicesLength = reader.ReadUInt32();
                _reader = GetReader<ushort>(indicesLength, inMemorySize, stream);

#if DEBUG
                foreach (var item in _reader.Values)
                    Debug.Assert(item < _stringTable.Length);
#endif
            }

            IEnumerable<object> ICanEnumerate.Values => Values;
            public IEnumerable<string> Values => _reader.Values.Select(i => _stringTable[i]);
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                EncodedStreamWriter.StringEncoder.WriteTo(
                    (uint)_stringTable.Length,
                    _stringTable,
                    _reader.Size,
                    _reader.Values,
                    writer
                );
            }

            public string[] StringTable => _stringTable;
        }

        public class StructDecoder<T> : ICanEnumerateWithSize<T>, ICanWriteToBinaryWriter, IHaveStringTable
            where T : struct
        {
            readonly ICanEnumerateWithSize<ushort> _reader;
            readonly T[] _table;

            public StructDecoder(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                var len = reader.ReadUInt32();
                _table = stream.ReadArray<T>(len);
                _reader = GetReader<ushort>(reader.ReadUInt32(), inMemorySize, stream);
            }

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values => _reader.Values.Select(i => _table[i]);
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                EncodedStreamWriter.StructEncoder<T>.WriteTo(_table, _reader.Size, _reader.Values, writer);
            }

            public string[] StringTable => _table.Select(d => d.ToString()!).ToArray();
        }

        public class ObjectReader<T> : ICanEnumerateWithSize<T>, ICanWriteToBinaryWriter
            where T : ICanInitializeFromBinaryReader, ICanWriteToBinaryWriter
        {
            readonly BrightDataContext _context;
            readonly ICanEnumerateWithSize<T> _reader;

            public ObjectReader(BrightDataContext context, BinaryReader reader, Stream stream, uint inMemorySize)
            {
                _context = context;
                _reader = GetReader(reader.ReadUInt32(), inMemorySize, reader, stream, Create);
            }

            T Create(BinaryReader reader) => _context.Create<T>(reader);

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values => _reader.Values;
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                EncodedStreamWriter.ObjectWriter<T>.WriteTo(_reader.Size, _reader.Values, writer);
            }
        }

        public class StructReader<T> : ICanEnumerateWithSize<T>, ICanWriteToBinaryWriter
            where T : struct
        {
            readonly ICanEnumerateWithSize<T> _reader;

            public StructReader(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                _reader = GetReader<T>(reader.ReadUInt32(), inMemorySize, stream);
            }

            IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
            public IEnumerable<T> Values => _reader.Values;
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                EncodedStreamWriter.StructWriter<T>.WriteTo(_reader.Size, _reader.Values, writer);
            }
        }

        public class StringReader : ICanEnumerateWithSize<string>, ICanWriteToBinaryWriter
        {
            readonly ICanEnumerateWithSize<string> _reader;

            public StringReader(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                _reader = GetReader(reader.ReadUInt32(), inMemorySize, reader, stream, r => r.ReadString());
            }

            IEnumerable<object> ICanEnumerate.Values => Values;
            public IEnumerable<string> Values => _reader.Values;
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                EncodedStreamWriter.StringWriter.WriteTo(_reader.Size, _reader.Values, writer);
            }
        }
    }
}
