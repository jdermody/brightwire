using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class FlatVectorIndex<T>(IStoreVectors<T> storage, DistanceMetric distanceMetric) : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        public IStoreVectors<T> Storage { get; } = storage;
        public uint Add(ReadOnlySpan<T> vector) => Storage.Add(vector);

        public void Dispose()
        {
            Storage.Dispose();
        }

        public unsafe IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
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

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            var size = Storage.Size;
            var distance = GetDistance(vector);

            // find the closest input vector index for each vector in the set
            var ret = new uint[size];
            Parallel.For(0, size, i => ret[i] = ((ReadOnlySpan<T>)distance.AsSpan2D().GetRowSpan((int)i)).MinimumIndex());
            return ret;
        }

        /// <summary>
        /// Find distance between each vector in the set and each input vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        T[,] GetDistance(ReadOnlyMemory<T>[] vector)
        {
            var size = Storage.Size;
            var ret = new T[size, vector.Length];
            Parallel.For(0, size * vector.Length, i =>
            {
                var dataIndex = (uint)i % size;
                var vectorIndex = (uint)i / size;
                ret[dataIndex, vectorIndex] = Storage[dataIndex].FindDistance(vector[vectorIndex].Span, distanceMetric);
            });
            return ret;
        }
    }
}
