using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrightData.Buffers
{
    public static class BufferWriter
    {
        public static void CopyTo<T>(IHybridBuffer<T> buffer, Stream stream)
        {
            var writer = GetWriter(buffer);
            using var binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.WriteTo(binaryWriter);
        }

        static ICanWriteToBinaryWriter GetWriter<T>(IHybridBuffer<T> buffer)
        {
            var typeOfT = typeof(T);
            var shouldEncode = buffer.NumDistinct.HasValue && buffer.NumDistinct.Value < buffer.Length / 2;

            if (typeOfT == typeof(string)) {
                var stringBuffer = (IHybridBuffer<string>)buffer;
                return shouldEncode
                    ? (ICanWriteToBinaryWriter)new StringEncoder(stringBuffer)
                    : new StringWriter(stringBuffer);
            }

            if (typeOfT.IsValueType) {
                var writerType = shouldEncode ? typeof(StructEncoder<>) : typeof(StructWriter<>);
                return (ICanWriteToBinaryWriter) Activator.CreateInstance(writerType.MakeGenericType(typeOfT), buffer);
            }

            return (ICanWriteToBinaryWriter) Activator.CreateInstance(typeof(ObjectWriter<>).MakeGenericType(typeOfT), buffer);
        }

        internal class StringEncoder : ICanWriteToBinaryWriter
        {
            readonly Dictionary<string, ushort> _table = new Dictionary<string, ushort>();
            readonly IHybridBuffer<string> _buffer;

            public StringEncoder(IHybridBuffer<string> buffer)
            {
                _buffer = buffer;

                // encode the values
                foreach (var item in buffer.EnumerateTyped()) {
                    if (!_table.ContainsKey(item))
                        _table.Add(item, (ushort)_table.Count);
                }
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(
                    (uint) _table.Count,
                    _table.OrderBy(kv => kv.Value).Select(kv => kv.Key),
                    _buffer.Length,
                    _buffer.EnumerateTyped().Select(v => _table[v]),
                    writer
                );
            }

            internal static void WriteTo(uint stringTableCount, IEnumerable<string> stringTable, uint indexCount, IEnumerable<ushort> indices, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)BufferType.EncodedString);

                // write the length
                writer.Write(stringTableCount);

                // write the strings
                foreach (var str in stringTable)
                    writer.Write(str);

                // write the buffer length and the indices
                writer.Write(indexCount);
                writer.Flush();
                WriteBuffered(indices, indexCount, writer.BaseStream);
            }
        }

        internal class StructEncoder<T> : ICanWriteToBinaryWriter where T : struct
        {
            private readonly Dictionary<T, ushort> _table = new Dictionary<T, ushort>();
            readonly IHybridBuffer<T> _buffer;

            public StructEncoder(IHybridBuffer<T> buffer)
            {
                _buffer = buffer;

                // encode the values
                foreach (var item in buffer.EnumerateTyped()) {
                    if (!_table.ContainsKey(item))
                        _table.Add(item, (ushort)_table.Count);
                }
            }

            public void WriteTo(BinaryWriter writer)
            {
                var data = _table.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray();
                WriteTo(
                    data, 
                    _buffer.Length, 
                    _buffer.EnumerateTyped().Select(v => _table[v]),
                    writer
                );
            }

            internal static void WriteTo(T[] keys, uint indexCount, IEnumerable<ushort> indices, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)BufferType.EncodedStruct);

                // write the length
                writer.Write((uint)keys.Length);
                writer.Flush();

                // write the data
                writer.BaseStream.Write(MemoryMarshal.Cast<T, byte>(keys));

                // write the buffer length and the indices
                writer.Write(indexCount);
                writer.Flush();
                WriteBuffered(indices, indexCount, writer.BaseStream);
            }
        }

        internal class ObjectWriter<T> : ICanWriteToBinaryWriter
            where T : ICanWriteToBinaryWriter
        {
            private readonly IHybridBuffer<T> _buffer;

            public ObjectWriter(IHybridBuffer<T> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(_buffer.Length, _buffer.EnumerateTyped(), writer);
            }

            internal static void WriteTo(uint itemsCount, IEnumerable<T> items, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)BufferType.Object);

                // write the number of items
                writer.Write(itemsCount);

                // write the items
                foreach (var item in items)
                    item.WriteTo(writer);
            }
        }

        internal class StringWriter : ICanWriteToBinaryWriter
        {
            private readonly IHybridBuffer<string> _buffer;

            public StringWriter(IHybridBuffer<string> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(_buffer.Length, _buffer.EnumerateTyped(), writer);
            }

            internal static void WriteTo(uint itemCount, IEnumerable<string> items, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)BufferType.String);

                // write the number of items
                writer.Write(itemCount);

                // write the items
                foreach (var item in items)
                    writer.Write(item);
            }
        }

        internal class StructWriter<T> : ICanWriteToBinaryWriter
            where T : struct
        {
            private readonly IHybridBuffer<T> _buffer;

            public StructWriter(IHybridBuffer<T> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(_buffer.Length, _buffer.EnumerateTyped(), writer);
            }

            internal static void WriteTo(uint numItems, IEnumerable<T> items, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)BufferType.Struct);

                // write the items
                writer.Write(numItems);
                writer.Flush();
                WriteBuffered(items, numItems, writer.BaseStream);
            }
        }

        static void WriteBuffered<T>(IEnumerable<T> reader, uint count, Stream stream, uint tempBufferSize = 8096)
            where T : struct
        {
            var temp = new T[tempBufferSize];
            var index = 0;

            using var enumerator = reader.GetEnumerator();
            uint i = 0;
            for (; i < count && enumerator.MoveNext(); i++) {
                var val = enumerator.Current;
                if (index == tempBufferSize) {
                    stream.Write(MemoryMarshal.Cast<T, byte>(temp));
                    index = 0;
                }

                temp[index++] = val;
            }
#if DEBUG
            Debug.Assert(i == count);
#endif
            if (index > 0)
                stream.Write(MemoryMarshal.Cast<T, byte>(((ReadOnlySpan<T>)temp).Slice(0, index)));
        }
    }
}
