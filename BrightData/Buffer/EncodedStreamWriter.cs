using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BrightData.Helper;

namespace BrightData.Buffer
{
    /// <summary>
    /// Writes hybrid buffers to binary writers, potentially encoding along the way
    /// </summary>
    public static class EncodedStreamWriter
    {
        /// <summary>
        /// Writes the hybrid buffer to a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">Buffer to write</param>
        /// <param name="stream">Stream to write to</param>
        public static void CopyTo<T>(IHybridBuffer<T> buffer, Stream stream) where T : notnull
        {
            var shouldEncode = buffer.NumDistinct.HasValue && buffer.NumDistinct.Value < buffer.Size / 2;
            var writer = GetWriter(buffer, shouldEncode);
            using var binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.WriteTo(binaryWriter);
        }

        /// <summary>
        /// Returns an writer that can write the buffer to a binary writer
        /// </summary>
        /// <param name="buffer">Buffer to write</param>
        /// <param name="shouldEncode">If the values should be encoded</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ICanWriteToBinaryWriter GetWriter<T>(ICanEnumerate<T> buffer, bool shouldEncode) where T : notnull
        {
            var typeOfT = typeof(T);

            if (typeOfT == typeof(string)) {
                var stringBuffer = (ICanEnumerate<string>)buffer;
                return shouldEncode
                    ? new StringEncoder(stringBuffer)
                    : new StringWriter(stringBuffer);
            }

            if (typeOfT.IsValueType) {
                var writerType = shouldEncode ? typeof(StructEncoder<>) : typeof(StructWriter<>);
                return GenericActivator.Create<ICanWriteToBinaryWriter>(writerType.MakeGenericType(typeOfT), buffer);
            }

            return GenericActivator.Create<ICanWriteToBinaryWriter>(typeof(ObjectWriter<>).MakeGenericType(typeOfT), buffer);
        }

        internal class StringEncoder : ICanWriteToBinaryWriter
        {
            readonly Dictionary<string, ushort> _table = new();
            readonly ICanEnumerate<string> _buffer;

            public StringEncoder(ICanEnumerate<string> buffer)
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
                    _buffer.Size,
                    _buffer.EnumerateTyped().Select(v => _table[v]),
                    writer
                );
            }

            internal static void WriteTo(uint stringTableCount, IEnumerable<string> stringTable, uint indexCount, IEnumerable<ushort> indices, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)HybridBufferType.EncodedString);

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
            readonly Dictionary<T, ushort> _table = new();
            readonly ICanEnumerate<T> _buffer;

            public StructEncoder(ICanEnumerate<T> buffer)
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
                    _buffer.Size, 
                    _buffer.EnumerateTyped().Select(v => _table[v]),
                    writer
                );
            }

            internal static void WriteTo(T[] keys, uint indexCount, IEnumerable<ushort> indices, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)HybridBufferType.EncodedStruct);

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
            readonly ICanEnumerate<T> _buffer;

            public ObjectWriter(ICanEnumerate<T> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(_buffer.Size, _buffer.EnumerateTyped(), writer);
            }

            internal static void WriteTo(uint itemsCount, IEnumerable<T> items, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)HybridBufferType.Object);

                // write the number of items
                writer.Write(itemsCount);

                // write the items
                foreach (var item in items)
                    item.WriteTo(writer);
            }
        }

        internal class StringWriter : ICanWriteToBinaryWriter
        {
            readonly ICanEnumerate<string> _buffer;

            public StringWriter(ICanEnumerate<string> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(_buffer.Size, _buffer.EnumerateTyped(), writer);
            }

            internal static void WriteTo(uint itemCount, IEnumerable<string> items, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)HybridBufferType.String);

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
            readonly ICanEnumerate<T> _buffer;

            public StructWriter(ICanEnumerate<T> buffer)
            {
                _buffer = buffer;
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteTo(_buffer.Size, _buffer.EnumerateTyped(), writer);
            }

            internal static void WriteTo(uint numItems, IEnumerable<T> items, BinaryWriter writer)
            {
                // write the buffer type
                writer.Write((byte)HybridBufferType.Struct);

                // write the items
                writer.Write(numItems);
                writer.Flush();
                WriteBuffered(items, numItems, writer.BaseStream);
            }
        }

        static void WriteBuffered<T>(IEnumerable<T> reader, uint count, Stream stream, uint tempBufferSize = 8192)
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
                stream.Write(MemoryMarshal.Cast<T, byte>(((ReadOnlySpan<T>)temp)[..index]));
        }
    }
}
