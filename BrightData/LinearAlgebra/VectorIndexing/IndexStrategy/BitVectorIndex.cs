using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BrightData.Types;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class BitVectorIndex<T>(IStoreVectors<T> storage) : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly ArrayPoolBufferWriter<ulong> _indexStorage = new();
        readonly uint _bitVectorWords = BitVector.GetRequiredSize(storage.VectorSize);

        public IStoreVectors<T> Storage { get; } = storage;

        public void Dispose()
        {
            Storage.Dispose();
            _indexStorage.Dispose();
        }

        public uint Add(ReadOnlySpan<T> vector)
        {
            _indexStorage.Write(AsBits(vector).AsSpan());
            var ret = Storage.Add(vector);
            return ret;
        }

        static BitVector AsBits(ReadOnlySpan<T> vector)
        {
            var index = 0;
            var ret = new BitVector((uint)vector.Length);
            foreach (var value in vector) {
                if (value > T.Zero)
                    ret[index] = true;
                ++index;
            }
            return ret;
        }

        public IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
        {
            var len = Storage.Size;
            var results = new uint[len];
            var span = _indexStorage.WrittenSpan;
            var comparison = AsBits(vector);
            for (var i = 0U; i < len; i++) {
                var existing = new ReadOnlyBitVector(span.Slice((int)(i * _bitVectorWords), (int)_bitVectorWords), _bitVectorWords);
                results[i] = existing.HammingDistance(comparison);
            }
            return results
                .Select((x, i) => (Distance: x, Index: (uint)i))
                .OrderBy(x => x.Distance)
                .Select(x => x.Index)
            ;
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            throw new NotImplementedException();
        }
    }
}
