using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrightData.Buffers2
{
    public static class BufferWriter
    {
        public static void CopyTo<T>(IHybridBuffer<T> buffer, Stream stream)
        {
            var writer = GetWriter(buffer);
            writer.WriteTo(stream);
        }

        static IWriteToStream GetWriter<T>(IHybridBuffer<T> buffer)
        {
            var typeOfT = typeof(T);
            var shouldEncode = buffer.NumDistinct.HasValue && buffer.NumDistinct.Value < buffer.Length / 2;

            if (typeOfT == typeof(string)) {
                var stringBuffer = (IHybridBuffer<string>)buffer;
                return shouldEncode
                    ? (IWriteToStream)new StringEncoder(stringBuffer)
                    : new StringWriter(stringBuffer);
            }

            if (typeOfT.IsValueType) {
                var writerType = shouldEncode ? typeof(StructEncoder<>) : typeof(StructWriter<>);
                return (IWriteToStream) Activator.CreateInstance(writerType, buffer);
            }

            return (IWriteToStream) Activator.CreateInstance(typeof(ObjectWriter<>), buffer);
        }

        interface IWriteToStream
        {
            void WriteTo(Stream stream);
        }

        class StringEncoder : IWriteToStream
        {
            readonly Dictionary<string, uint> _table = new Dictionary<string, uint>();
            readonly IHybridBuffer<string> _buffer;

            public StringEncoder(IHybridBuffer<string> buffer)
            {
                _buffer = buffer;

                // encode the values
                foreach (var item in buffer.EnumerateTyped()) {
                    if (!_table.ContainsKey(item))
                        _table.Add(item, (uint)_table.Count);
                }
            }

            public void WriteTo(Stream stream)
            {
                using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

                // write the buffer type
                writer.Write((byte)BufferType.EncodedString);

                // write the length
                writer.Write((uint)_table.Count);

                // write the strings
                foreach (var item in _table.OrderBy(kv => kv.Value))
                    writer.Write(item.Key);

                // write the buffer length and the indices
                writer.Write(_buffer.Length);
                writer.Flush();
                WriteBuffered<uint>(_buffer.EnumerateTyped().Select(v => _table[v]), stream);
            }
        }

        class StructEncoder<T> : IWriteToStream where T : struct
        {
            private readonly Dictionary<T, uint> _table = new Dictionary<T, uint>();
            readonly IHybridBuffer<T> _buffer;

            public StructEncoder(IHybridBuffer<T> buffer)
            {
                _buffer = buffer;

                // encode the values
                foreach (var item in buffer.EnumerateTyped()) {
                    if (!_table.ContainsKey(item))
                        _table.Add(item, (uint)_table.Count);
                }
            }

            public void WriteTo(Stream stream)
            {
                using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

                // write the buffer type
                writer.Write((byte)BufferType.EncodedStruct);

                // write the length
                writer.Write((uint)_table.Count);

                // write the data
                var data = _table.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray();
                stream.Write(MemoryMarshal.Cast<T, byte>(data));

                // write the buffer length and the indices
                writer.Write(_buffer.Length);
                writer.Flush();
                WriteBuffered(_buffer.EnumerateTyped().Select(v => _table[v]), stream);
            }
        }

        class ObjectWriter<T> : IWriteToStream
            where T : ICanWriteToBinaryWriter
        {
            private readonly IHybridBuffer<T> _buffer;

            public ObjectWriter(IHybridBuffer<T> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(Stream stream)
            {
                using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

                // write the buffer type
                writer.Write((byte)BufferType.Object);

                // write the number of items
                writer.Write(_buffer.Length);

                // write the items
                foreach (var item in _buffer.EnumerateTyped())
                    item.WriteTo(writer);
            }
        }

        class StringWriter : IWriteToStream
        {
            private readonly IHybridBuffer<string> _buffer;

            public StringWriter(IHybridBuffer<string> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(Stream stream)
            {
                using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

                // write the buffer type
                writer.Write((byte)BufferType.String);

                // write the number of items
                writer.Write(_buffer.Length);

                // write the items
                foreach (var item in _buffer.EnumerateTyped())
                    writer.Write(item);
            }
        }
        class StructWriter<T> : IWriteToStream
            where T : struct
        {
            private readonly IHybridBuffer<T> _buffer;

            public StructWriter(IHybridBuffer<T> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(Stream stream)
            {
                using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

                // write the buffer type
                writer.Write((byte)BufferType.Struct);

                // write the items
                writer.Write(_buffer.Length);
                writer.Flush();
                WriteBuffered<T>(_buffer.EnumerateTyped(), stream);
            }
        }

        static void WriteBuffered<T>(IEnumerable<T> reader, Stream stream, uint tempBufferSize = 8096)
            where T : struct
        {
            var temp = new T[tempBufferSize];
            var index = 0;
            foreach (var item in reader) {
                if (index == tempBufferSize) {
                    stream.Write(MemoryMarshal.Cast<T, byte>(temp));
                    index = 0;
                }

                temp[index++] = item;
            }
            if (index > 0)
                stream.Write(MemoryMarshal.Cast<T, byte>(((ReadOnlySpan<T>)temp).Slice(0, index)));
        }
    }
}
