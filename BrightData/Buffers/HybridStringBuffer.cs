using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BrightData;
using BrightData.Buffers;
using BrightData.Helper;

namespace BrightTable.Buffers
{
    public class HybridStringBuffer : HybridBufferBase<string>
    {
        //private readonly IBrightDataContext _context;
        //private readonly uint _index;
        //private readonly TempStreamManager _tempStreams;
        //readonly List<string> _buffer = new List<string>();
        //private uint _size;
        //private bool _isFinal = false, _hasWrittenToStream = false;

        //public HybridStringBuffer(IBrightDataContext context, uint index, TempStreamManager tempStreams)
        //{
        //    _context = context;
        //    _index = index;
        //    _tempStreams = tempStreams;
        //}

        //public void WriteTo(BinaryWriter writer)
        //{
        //    if (_hasWrittenToStream) {
        //        var stream = _tempStreams.Get(_index);
        //        lock (stream) {
        //            stream.Seek(0, SeekOrigin.Begin);
        //            stream.CopyTo(writer.BaseStream);
        //        }
        //    }
        //    foreach(var item in _buffer)
        //        writer.Write(item);
        //}

        //public IEnumerable<object> Enumerate()
        //{
        //    if (_hasWrittenToStream) {
        //        var stream = _tempStreams.Get(_index);
        //        lock (stream) {
        //            stream.Seek(0, SeekOrigin.Begin);
        //            var reader = new BinaryReader(stream);
        //            while (stream.Position < stream.Length)
        //                yield return reader.ReadString();
        //        }
        //    }

        //    foreach (var item in _buffer)
        //        yield return item;
        //}

        //public uint Size => _size + (uint)_buffer.Count;
        //public bool IsEncoded => false;

        //void IAutoGrowBuffer.Add(object obj) => Add(obj.ToString());
        //public void Add(string str)
        //{
        //    if (_isFinal)
        //        throw new Exception("Buffer has been finalized");

        //    if (_buffer.Count == BUFFER_SIZE) {
        //        var stream = _tempStreams.Get(_index);
        //        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        //        foreach (var item in _buffer)
        //            writer.Write(item);
        //        _size += (uint)_buffer.Count;
        //        _buffer.Clear();
        //    }

        //    _buffer.Add(str);
        //}

        //public void Finalise()
        //{
        //    _isFinal = true;
        //}
        public HybridStringBuffer(IBrightDataContext context, uint index, TempStreamManager tempStreams, uint bufferSize = 32768) 
            : base(context, index, tempStreams, bufferSize)
        {
        }

        public override void Write(IReadOnlyCollection<string> items, BinaryWriter writer)
        {
            foreach (var item in items)
                writer.Write(item);
        }

        protected override IEnumerable<string> _Read(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            while (stream.Position < stream.Length)
                yield return reader.ReadString();
        }
    }
}
