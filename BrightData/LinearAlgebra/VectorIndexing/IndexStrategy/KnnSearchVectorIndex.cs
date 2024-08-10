using System;
using System.Collections.Generic;
using System.Numerics;
using BrightData.Types;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class KnnSearchVectorIndex<T>(IStoreVectors<T> vectors, Func<IStoreVectors<T>, ISupportKnnSearch<T>> creator) : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ISupportKnnSearch<T>? _search = null;
        readonly object _lock = new();

        public void Dispose()
        {
            // nop
        }

        public IStoreVectors<T> Storage => vectors;

        public uint Add(ReadOnlySpan<T> vector) => Storage.Add(vector);

        public IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
        {
            var results = GetSearch().KnnSearch<FixedSizeSortedAscending32Array<uint, T>>(vector);
            return results.EnumerateValues<uint, FixedSizeSortedAscending32Array<uint, T>, T>();
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            var ret = new uint[vector.Length];
            var search = GetSearch();
            Storage.ForEach((x, vi) => {
                foreach (var v in vector) {
                    var results = search.KnnSearch<FixedSizeSortedAscending1Array<uint, T>>(v.Span);
                    ret[vi] = results.MinValue;
                }
            });
            return ret;
        }

        ISupportKnnSearch<T> GetSearch()
        {
            if (_search is null) {
                lock (_lock) {
                    _search ??= creator(Storage);
                }
            }
            return _search;
        }
    }
}
