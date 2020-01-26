using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData;
using BrightData.Buffers;
using BrightData.Helper;

namespace BrightTable.Buffers
{
    public class HybridStructBuffer<T> : HybridBufferBase<T>
        where T : struct
    {
        //private const int BUFFER_SIZE = 32768;

        //private readonly IBrightDataContext _context;
        //private readonly uint _index;
        //private readonly TempStreamManager _tempStreams;
        //readonly T[] _buffer = new T[BUFFER_SIZE];
        //private uint _size, _pos;
        //private bool _isFinal = false, _hasWrittenToStream = false;

        //public HybridStructBuffer(IBrightDataContext context, uint index, ColumnType type, IMetaData metaData, TempStreamManager tempStreams)
        //{
        //    _context = context;
        //    _index = index;
        //    _tempStreams = tempStreams;
        //    MetaData = metaData;
        //    SingleType = type;
        //}

        //public void Dispose()
        //{
        //    // nop
        //}

        //public IMetaData MetaData { get; }
        //public ColumnType SingleType { get; }

        //public void WriteTo(BinaryWriter writer)
        //{
        //    if (_hasWrittenToStream) {
        //        var stream = _tempStreams.Get(_index);
        //        lock (stream) {
        //            stream.Seek(0, SeekOrigin.Begin);
        //            stream.CopyTo(writer.BaseStream);
        //        }
        //    }
        //    if (_pos > 0) {
        //        var ptr = MemoryMarshal.Cast<T, byte>(_buffer.AsSpan().Slice(0, (int)_pos));
        //        writer.BaseStream.Write(ptr);
        //    }
        //}

        //public IEnumerable<T> EnumerateTyped()
        //{
        //    if (_hasWrittenToStream) {
        //        var stream = _tempStreams.Get(_index);
        //        var size = Unsafe.SizeOf<T>();
        //        lock (stream) {
        //            stream.Seek(0, SeekOrigin.Begin);
        //            while (stream.Position < stream.Length) {
        //                int len = stream.Read(MemoryMarshal.Cast<T, byte>(_buffer)) / size;
        //                for (var i = 0; i < len; i++)
        //                    yield return _buffer[i];
        //            }
        //        }
        //    }

        //    for (uint i = 0; i < _pos; i++)
        //        yield return _buffer[i];
        //}

        //public IEnumerable<object> Enumerate()
        //{
        //    foreach (var item in EnumerateTyped())
        //        yield return item;
        //}

        //public uint Size => _size + _pos;
        //public bool IsEncoded => false;

        //void IAutoGrowBuffer.Add(object obj) => Add((T) obj);
        //public void Add(T typedObject)
        //{
        //    if(_isFinal)
        //        throw new Exception("Buffer has been finalized");

        //    if (_pos == BUFFER_SIZE) {
        //        var ptr = MemoryMarshal.Cast<T, byte>(_buffer);
        //        var stream = _tempStreams.Get(_index);
        //        stream.Write(ptr);
        //        _size += _pos;
        //        _pos = 0;
        //    }

        //    _buffer[_pos++] = typedObject;
        //}

        //public void Finalise()
        //{
        //    _isFinal = true;
        //}
        readonly int _itemSize;

        public HybridStructBuffer(IBrightDataContext context, uint index, TempStreamManager tempStreams, uint bufferSize = 32768)
            : base(context, index, tempStreams, bufferSize)
        {
            _itemSize = Unsafe.SizeOf<T>();
        }

        public override void Write(IReadOnlyCollection<T> items, BinaryWriter writer)
        {
            var all = items.ToArray();
            var ptr = MemoryMarshal.Cast<T, byte>(all);
            writer.BaseStream.Write(ptr);
        }

        protected override IEnumerable<T> _Read(Stream stream)
        {
            var data = new byte[stream.Length];
            stream.Read(data);
            var len = stream.Length / _itemSize;
            for (var i = 0; i < len; i++) {
                yield return MemoryMarshal.Cast<byte, T>(data)[i];
            }
        }
    }
}
