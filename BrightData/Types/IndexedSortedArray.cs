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

        public T this[int position] => _values[position];
        public T this[uint position] => _values[(int)position];

        public void Add(T value)
        {
            _values.Add(value);
            if(_values.Count > 1)
                SortedArrayHelper.InsertIndexed(Size - 1, value, Values);
        }

        public ref T Find(uint itemIndex)
        {
            var arrayIndex = Values.BinarySearch(new Comparer(itemIndex));
            if (arrayIndex >= 0)
                return ref Values[arrayIndex];
            return ref Unsafe.NullRef<T>();
        }

        public bool TryFind(uint itemIndex, [NotNullWhen(true)]out T? value)
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
