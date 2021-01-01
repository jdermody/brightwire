﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace BrightData.Buffers
{
    public static class BufferedStreamReader
    {
        public static ICanEnumerate<T> GetReader<T>(Stream stream, IBrightDataContext context, uint inMemorySize)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8);
            var type = (BufferType)reader.ReadByte();

            if (type == BufferType.EncodedString)
                return (ICanEnumerate<T>)new StringDecoder(reader, stream, inMemorySize);

            if (type == BufferType.EncodedStruct)
                return (ICanEnumerate<T>) Activator.CreateInstance(typeof(StructDecoder<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize);

            if(type == BufferType.Object)
                return (ICanEnumerate<T>)Activator.CreateInstance(typeof(ObjectReader<>).MakeGenericType(typeof(T)), context, reader, stream, inMemorySize);

            if (type == BufferType.String)
                return (ICanEnumerate<T>) new StringReader(reader, stream, inMemorySize);

            if (type == BufferType.Struct)
                return (ICanEnumerate<T>)Activator.CreateInstance(typeof(StructReader<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize);

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

        class ReadFromRepeatableStream<T> : ICanEnumerate<T>
            where T: struct
        {
            readonly RepeatableStreamReader _stream;

            public ReadFromRepeatableStream(uint length, Stream stream)
            {
                Size = length;
                _stream = new RepeatableStreamReader(stream);
            }

            public IEnumerable<T> EnumerateTyped() => BufferedRead<T>(_stream.GetStream(), Size);
            public uint Size { get; }
        }

        class ConvertFromStream<T> : ICanEnumerate<T>
        {
            readonly Func<BinaryReader, T> _converter;
            readonly RepeatableStreamReader _stream;

            public ConvertFromStream(uint length, Stream stream, Func<BinaryReader, T> converter)
            {
                Size = length;
                _converter = converter;
                _stream = new RepeatableStreamReader(stream);
            }

            public IEnumerable<T> EnumerateTyped()
            {
                var reader = _stream.GetReader();
                for (uint i = 0; i < Size; i++)
                    yield return _converter(reader);
            }

            public uint Size { get; }
        }

        class ReadFromMemory<T> : ICanEnumerate<T>
            where T: struct
        {
            private readonly T[] _data;

            public ReadFromMemory(uint length, Stream stream)
            {
                _data = BufferedRead<T>(stream, length).ToArray();
#if DEBUG
                Debug.Assert(_data.Length == length);
#endif
            }

            public IEnumerable<T> EnumerateTyped() => _data;
            public uint Size => (uint)_data.Length;
        }

        class LoadIntoMemory<T> : ICanEnumerate<T>
        {
            private readonly T[] _data;

            public LoadIntoMemory(uint length, BinaryReader reader, Func<BinaryReader, T> objectBuilder)
            {
                _data = new T[length];
                for (uint i = 0; i < length; i++)
                    _data[i] = objectBuilder(reader);
            }

            public IEnumerable<T> EnumerateTyped() => _data;
            public uint Size => (uint)_data.Length;
        }

        static ICanEnumerate<T> _GetReader<T>(uint length, uint inMemorySize, Stream stream)
            where T: struct
        {
            if(length <= inMemorySize)
                return new ReadFromMemory<T>(length, stream);
            return new ReadFromRepeatableStream<T>(length, stream);
        }

        static ICanEnumerate<T> _GetReader<T>(uint length, uint inMemorySize, BinaryReader reader, Stream stream, Func<BinaryReader, T> objectBuilder)
        {
            if (length <= inMemorySize)
                return new LoadIntoMemory<T>(length, reader, objectBuilder);
            return new ConvertFromStream<T>(length, stream, objectBuilder);
        }

        class StringDecoder : ICanEnumerate<string>, ICanWriteToBinaryWriter
        {
            private readonly ICanEnumerate<ushort> _reader;
            readonly string[] _stringTable;

            public StringDecoder(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                var len = reader.ReadUInt32();
                _stringTable = new string[len];
                for (uint i = 0; i < len; i++)
                    _stringTable[i] = reader.ReadString();
                var indicesLength = reader.ReadUInt32();
                _reader = _GetReader<ushort>(indicesLength, inMemorySize, stream);

#if DEBUG
                int offset = 0;
                foreach (var item in _reader.EnumerateTyped()) {
                    Debug.Assert(item < _stringTable.Length);
                    ++offset;
                }
#endif
            }

            public IEnumerable<string> EnumerateTyped() => _reader.EnumerateTyped().Select(i => _stringTable[i]);
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                BufferWriter.StringEncoder.WriteTo(
                    (uint)_stringTable.Length, 
                    _stringTable, 
                    _reader.Size, 
                    _reader.EnumerateTyped(),
                    writer
                );
            }
        }

        class StructDecoder<T> : ICanEnumerate<T>, ICanWriteToBinaryWriter
            where T : struct
        {
            private readonly ICanEnumerate<ushort> _reader;
            readonly T[] _table;

            public StructDecoder(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                var len = reader.ReadUInt32();
                _table = new T[len];
                stream.Read(MemoryMarshal.Cast<T, byte>(_table));
                _reader = _GetReader<ushort>(reader.ReadUInt32(), inMemorySize, stream);
            }

            public IEnumerable<T> EnumerateTyped() => _reader.EnumerateTyped().Select(i => _table[i]);
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                BufferWriter.StructEncoder<T>.WriteTo(_table, _reader.Size, _reader.EnumerateTyped(), writer);
            }
        }

        class ObjectReader<T> : ICanEnumerate<T>, ICanWriteToBinaryWriter
            where T : ICanInitializeFromBinaryReader, ICanWriteToBinaryWriter
        {
            private readonly BrightDataContext _context;
            private readonly ICanEnumerate<T> _reader;

            public ObjectReader(BrightDataContext context, BinaryReader reader, Stream stream, uint inMemorySize)
            {
                _context = context;
                _reader = _GetReader(reader.ReadUInt32(), inMemorySize, reader, stream, _Create);
            }

            T _Create(BinaryReader reader)
            {
                var ret = (T)FormatterServices.GetUninitializedObject(typeof(T));
                ret.Initialize(_context, reader);
                return ret;
            }

            public IEnumerable<T> EnumerateTyped() => _reader.EnumerateTyped();
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                BufferWriter.ObjectWriter<T>.WriteTo(_reader.Size, _reader.EnumerateTyped(), writer);
            }
        }

        class StructReader<T> : ICanEnumerate<T>, ICanWriteToBinaryWriter
            where T : struct
        {
            readonly ICanEnumerate<T> _reader;

            public StructReader(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                _reader = _GetReader<T>(reader.ReadUInt32(), inMemorySize, stream);
            }

            public IEnumerable<T> EnumerateTyped() => _reader.EnumerateTyped();
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                BufferWriter.StructWriter<T>.WriteTo(_reader.Size, _reader.EnumerateTyped(), writer);
            }
        }

        class StringReader : ICanEnumerate<string>, ICanWriteToBinaryWriter
        {
            readonly ICanEnumerate<string> _reader;

            public StringReader(BinaryReader reader, Stream stream, uint inMemorySize)
            {
                _reader = _GetReader(reader.ReadUInt32(), inMemorySize, reader, stream, r => r.ReadString());
            }

            public IEnumerable<string> EnumerateTyped() => _reader.EnumerateTyped();
            public uint Size => _reader.Size;
            public void WriteTo(BinaryWriter writer)
            {
                BufferWriter.StringWriter.WriteTo(_reader.Size, _reader.EnumerateTyped(), writer);
            }
        }

        static IEnumerable<T> BufferedRead<T>(Stream stream, uint count, uint tempBufferSize = 8096)
            where T : struct
        {
            if (count < tempBufferSize) {
                // simple case
                var buffer = new T[count];
                stream.Read(MemoryMarshal.Cast<T, byte>(buffer));
                for(uint i = 0; i < count; i++)
                    yield return buffer[i];
            }
            else {
                // buffered read
                var temp = new T[tempBufferSize];
                var sizeofT = MemoryMarshal.Cast<T, byte>(temp).Length / (int) tempBufferSize;
                var totalRead = 0;

                while (totalRead < count) {
                    var remaining = count - totalRead;
                    var ptr = remaining >= tempBufferSize
                        ? temp
                        : ((Span<T>) temp).Slice(0, (int) remaining);
                    var readCount = stream.Read(MemoryMarshal.Cast<T, byte>(ptr)) / sizeofT;
                    for (var i = 0; i < readCount; i++)
                            yield return temp[i];
                    totalRead += readCount;
                }
            }
        }
    }
}