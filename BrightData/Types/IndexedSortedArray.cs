using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData.Types.Helper;

namespace BrightData.Types
{
    /// <summary>
    /// Sorted array of items that have a single index
    /// The index of each item is the sort key
    /// </summary>
    /// <param name="capacity"></param>
    /// <typeparam name="T"></typeparam>
    public class IndexedSortedArray<T>(int? capacity = null) : IHaveSize
        where T: unmanaged, IHaveSingleIndex
    {
        class Comparer(uint index) : IComparable<T>
        {
            public int CompareTo(T other) => index.CompareTo(other.Index);
        }
        readonly List<T> _values = capacity.HasValue ? new(capacity.Value) : [];

        /// <summary>
        /// Span of values
        /// </summary>
        public Span<T> Values => CollectionsMarshal.AsSpan(_values);

        /// <summary>
        /// Enumerable of indices
        /// </summary>
        public IEnumerable<uint> Indices => _values.Select(x => x.Index);

        /// <inheritdoc />
        public uint Size => (uint)Values.Length;

        /// <summary>
        /// Returns an item at the specified position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public T this[int position] => _values[position];

        /// <summary>
        /// Returns an item at the specified position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public T this[uint position] => _values[(int)position];

        /// <summary>
        /// Adds a new value
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            _values.Add(value);
            if(_values.Count > 1)
                SortedArrayHelper.InsertIndexed(Size - 1, value, Values);
        }

        /// <summary>
        /// Finds an item based on index
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        public ref T Find(uint itemIndex)
        {
            var arrayIndex = Values.BinarySearch(new Comparer(itemIndex));
            if (arrayIndex >= 0)
                return ref Values[arrayIndex];
            return ref Unsafe.NullRef<T>();
        }

        /// <summary>
        /// Tries to find an item based on index
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
