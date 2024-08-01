using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using BrightData.Types.Helper;

namespace BrightData.Types
{
    public class SortedArray<V, W>(int? capacity = null, bool isAscending = true) : IHaveSize
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        readonly List<V> _values = capacity.HasValue ? new(capacity.Value) : new();
        readonly List<W> _weights = capacity.HasValue ? new(capacity.Value) : new();

        public Span<V> Values => CollectionsMarshal.AsSpan(_values);
        public Span<W> Weights => CollectionsMarshal.AsSpan(_weights);
        public uint Size => (uint)Values.Length;

        public bool Add(W weight, in V item)
        {
            _values.Add(item);
            _weights.Add(weight);
            return isAscending
                ? SortedArrayHelper.InsertIntoAscending(false, Size-1, uint.MaxValue, item, weight, Values, Weights)
                : SortedArrayHelper.InsertIntoDescending(false, Size-1, uint.MaxValue, item, weight, Values, Weights)
            ;
        }

        public void RemoveAt(uint index)
        {
            _values.RemoveAt((int)index);
            _weights.RemoveAt((int)index);
        }

        public bool TryGet(W weight, [NotNullWhen(true)]out V? value)
        {
            var index = Weights.BinarySearch(weight);
            if (index >= 0) {
                value = _values[index];
                return true;
            }
            value = default;
            return false;
        }
    }
}
