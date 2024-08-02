using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData.Types.Helper;

namespace BrightData.Types
{
    public class IndexedSortedArray<T>(int? capacity = null) : IHaveSize
        where T: unmanaged, IHaveSingleIndex
    {
        class Comparer(uint index) : IComparable<T>
        {
            public int CompareTo(T other) => index.CompareTo(other.Index);
        }
        readonly List<T> _values = capacity.HasValue ? new(capacity.Value) : new();

        public Span<T> Values => CollectionsMarshal.AsSpan(_values);
        public uint Size => (uint)Values.Length;

        public bool Add(T value)
        {
            _values.Add(value);
            return SortedArrayHelper.InsertIndexed(Size - 1, value, Values);
        }

        public ref T Get(uint itemIndex)
        {
            var arrayIndex = Values.BinarySearch(new Comparer(itemIndex));
            if (arrayIndex >= 0)
                return ref Values[arrayIndex];
            return ref Unsafe.NullRef<T>();
        }

        public bool TryGet(uint itemIndex, [NotNullWhen(true)]out T? value)
        {
            var arrayIndex = Values.BinarySearch(new Comparer(itemIndex));
            if (arrayIndex >= 0) {
                value = _values[arrayIndex];
                return true;
            }
            value = default;
            return false;
        }
    }
}
