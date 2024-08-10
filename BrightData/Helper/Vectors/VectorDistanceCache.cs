using System;
using System.Collections.Concurrent;
using System.Numerics;

namespace BrightData.Helper.Vectors
{
    internal class VectorDistanceCache<T>(IStoreVectors<T> storage, DistanceMetric distanceMetric, VectorDistanceCache<T>? extensionOf = null) : IHaveSize, ICalculateNodeWeights<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly record struct Key(uint Index1, uint Index2);
        readonly ConcurrentDictionary<Key, T> _distanceCache = [];

        public uint VectorSize => storage.VectorSize;
        public uint Size => storage.Size + (extensionOf?.Size ?? 0);

        public void Clear(bool recursive = true)
        {
            _distanceCache.Clear();
            if (recursive && extensionOf != null)
                extensionOf.Clear();
        }

        public uint Add(ReadOnlySpan<T> vector)
        {
            var ret = Size;
            storage.Add(vector);
            return ret;
        }

        public ReadOnlySpan<T> Get(uint index)
        {
            if (extensionOf != null)
            {
                if (index < extensionOf.Size)
                    return extensionOf.Get(index);
                return storage[index - extensionOf.Size];
            }
            return storage[index];
        }

        public T GetDistance(uint index1, uint index2)
        {
            if (index1 == index2)
                return T.Zero;

            var key = index1 < index2
                ? new Key(index1, index2)
                : new Key(index2, index1);
            if (_distanceCache.TryGetValue(key, out var ret))
                return ret;

            var v1 = Get(index1);
            var v2 = Get(index2);
            _distanceCache[key] = ret = CalculateDistance(v1, v2, distanceMetric);
            return ret;
        }

        static T CalculateDistance(ReadOnlySpan<T> v1, ReadOnlySpan<T> v2, DistanceMetric distanceMetric) => v1.FindDistance(v2, distanceMetric);
        public T GetWeight(uint fromIndex, uint toIndex) => GetDistance(fromIndex, toIndex);
    }
}
