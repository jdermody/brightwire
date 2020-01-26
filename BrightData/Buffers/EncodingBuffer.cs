using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrightData.Buffers
{
    public class EncodingBuffer<T> : IAutoGrowBuffer<T>, IHaveEncodedData
    {
        private readonly IAutoGrowBuffer<T> _buffer;
        private readonly uint _maxEncodedSize;
        readonly Dictionary<T, uint> _dataTable = new Dictionary<T, uint>();
        readonly List<uint> _indices = new List<uint>();
        private uint _maxIndex = 0;

        public EncodingBuffer(IAutoGrowBuffer<T> buffer, uint maxEncodedSize = 32768)
        {
            _buffer = buffer;
            _maxEncodedSize = maxEncodedSize;
        }

        public void WriteTo(BinaryWriter writer)
        {
            if (IsEncoded) {
                var data = _dataTable.OrderBy(d => d.Value).Select(d => d.Key).ToList();

                writer.Write(data.Count);
                _buffer.Write(data, writer);

                Span<byte> ptr;
                if (_maxIndex <= byte.MaxValue) {
                    writer.Write((byte)8);
                    ptr = _indices.Select(v => (byte)v).ToArray();
                } else if (_maxIndex <= ushort.MaxValue) {
                    writer.Write((byte)16);
                    ptr = MemoryMarshal.Cast<ushort, byte>(_indices.Select(v => (ushort)v).ToArray());
                } else {
                    writer.Write((byte)32);
                    ptr = MemoryMarshal.Cast<uint, byte>(_indices.ToArray());
                }
                writer.Write(ptr);
            } else
                _buffer.WriteTo(writer);
            writer.Flush();
        }

        public uint Size => _buffer.Size;
        public void Add(object obj)
        {
            Add((T)obj);
        }

        public IEnumerable<object> Enumerate() => _buffer.Enumerate();

        public void Add(T obj)
        {
            _buffer.Add(obj);
            if (IsEncoded) {
                if (_dataTable.TryGetValue(obj, out var index))
                    _indices.Add(index);
                else if (_dataTable.Count == _maxEncodedSize) {
                    IsEncoded = false;
                    _dataTable.Clear();
                    _indices.Clear();
                } else {
                    _maxIndex = (uint)_dataTable.Count;
                    _dataTable.Add(obj, _maxIndex);
                    _indices.Add(_maxIndex);
                }
            }
        }

        public IEnumerable<T> EnumerateTyped() => _buffer.EnumerateTyped();
        public void Write(IReadOnlyCollection<T> items, BinaryWriter writer) => _buffer.Write(items, writer);
        public bool IsEncoded { get; private set; } = true;
    }
}
