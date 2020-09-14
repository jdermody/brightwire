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
