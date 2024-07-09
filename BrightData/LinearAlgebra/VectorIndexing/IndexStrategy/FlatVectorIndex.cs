﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class FlatVectorIndex<T>(IStoreVectors<T> storage) : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        public IStoreVectors<T> Storage { get; } = storage;
        public uint Add(ReadOnlySpan<T> vector) => Storage.Add(vector);

        public unsafe IEnumerable<uint> Rank(ReadOnlySpan<T> vector, DistanceMetric distanceMetric)
        {
            var results = new T[Storage.Size];
            fixed (T* ptr = vector) {
                var tempPtr = ptr;
                var len = vector.Length;
                Storage.ForEach((x, i) => results[i] = x.FindDistance(new ReadOnlySpan<T>(tempPtr, len), distanceMetric));
            }

            return results
                .Select((x, i) => (Distance: x, Index: (uint)i))
                .OrderBy(x => x.Distance)
                .Select(x => x.Index)
            ;
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector, DistanceMetric distanceMetric)
        {
            // find distance between each vector in the set and each input vector
            var size = Storage.Size;
            var distance = new T[size, vector.Length];
            Parallel.For(0, size * vector.Length, i =>
            {
                var dataIndex = (uint)i % size;
                var vectorIndex = (uint)i / size;
                distance[dataIndex, vectorIndex] = Storage[dataIndex].FindDistance(vector[vectorIndex].Span, distanceMetric);
            });

            // find the closest input vector index for each vector in the set
            var ret = new uint[size];
            Parallel.For(0, size, i => ret[i] = ((ReadOnlySpan<T>)distance.AsSpan2D().GetRowSpan((int)i)).MinimumIndex());
            return ret;
        }
    }
}
