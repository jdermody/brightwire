using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using BrightData.Types.Helper;
using System.Linq;

namespace BrightData.Types
{
    /// <summary>
    /// Fixed size sorted array of values and weights (max 1 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending1Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 1;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 1 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending1Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 1;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 2 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending2Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 2;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 2 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending2Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 2;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 3 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending3Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 3;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 3 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending3Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 3;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 4 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending4Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 4;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 4 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending4Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 4;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 5 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending5Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 5;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 5 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending5Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 5;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 6 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending6Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 6;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 6 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending6Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 6;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 7 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending7Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 7;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 7 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending7Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 7;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 8 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending8Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 8;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 8 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending8Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 8;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 9 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending9Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 9;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 9 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending9Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 9;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 10 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending10Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 10;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 10 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending10Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 10;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 11 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending11Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 11;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 11 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending11Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 11;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 12 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending12Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 12;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 12 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending12Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 12;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 13 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending13Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 13;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 13 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending13Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 13;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 14 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending14Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 14;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 14 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending14Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 14;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 15 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending15Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 15;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 15 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending15Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 15;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 16 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending16Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 16;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 16 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending16Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 16;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 17 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending17Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 17;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 17 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending17Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 17;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 18 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending18Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 18;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 18 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending18Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 18;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 19 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending19Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 19;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 19 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending19Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 19;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 20 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending20Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 20;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 20 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending20Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 20;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 21 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending21Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 21;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 21 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending21Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 21;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 22 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending22Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 22;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 22 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending22Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 22;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 23 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending23Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 23;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 23 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending23Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 23;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 24 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending24Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 24;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 24 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending24Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 24;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 25 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending25Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 25;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 25 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending25Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 25;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 26 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending26Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 26;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 26 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending26Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 26;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 27 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending27Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 27;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 27 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending27Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 27;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 28 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending28Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 28;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 28 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending28Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 28;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 29 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending29Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 29;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 29 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending29Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 29;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 30 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending30Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 30;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 30 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending30Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 30;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 31 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending31Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 31;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 31 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending31Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 31;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 32 elements)
    /// that is sorted in ascending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedAscending32Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 32;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[0] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[_size - 1] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoAscending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

    /// <summary>
    /// Fixed size sorted array of values and weights (max 32 elements)
    /// that is sorted in descending order based on each weight
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public record struct FixedSizeSortedDescending32Array<V, W>() : IFixedSizeSortedArray<V, W>
        where V : IComparable<V>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Max size of the array
        /// </summary>
        public const int MaxSize = 32;
        byte IFixedSizeSortedArray<V, W>.MaxSize => MaxSize;

        [InlineArray(MaxSize)]
        internal struct ValueArray
        {
            public V _element0;
        }
        [InlineArray(MaxSize)]
        internal struct WeightArray
        {
            public W _element0;
        }
        readonly ValueArray _values = new();
        readonly WeightArray _weights = new();
        byte _size = 0;

        /// <summary>
        /// Current number of elements
        /// </summary>
        public readonly byte Size => _size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        public readonly ReadOnlySpan<V> Values => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        public readonly ReadOnlySpan<W> Weights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);

        /// <summary>
        /// The smallest weight
        /// </summary>
        public readonly W MinWeight => _size > 0 ? Weights[_size - 1] : W.MaxValue;

        /// <summary>
        /// The largest weight
        /// </summary>
        public readonly W MaxWeight => _size > 0 ? Weights[0] : W.MinValue;

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        public readonly V? MinValue => _size > 0 ? Values[_size - 1] : default;

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        public readonly V? MaxValue => _size > 0 ? Values[0] : default;

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        public readonly (V Value, W Weight) this[byte index]
        {
            get
            {
                if (index < Size)
                    return (Values[index], Weights[index]);
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        public readonly IEnumerable<(V Value, W Weight)> Elements
        {
            get
            {
                for (byte i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a higher weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if elements should be unique - will return false if the value and weight already exists</param>
        /// <returns>True if the element was added</returns>
        public bool TryAdd(V value, W weight, bool enforceUnique = true)
        {
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), MaxSize);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), MaxSize);
            var ret = SortedArrayHelper.InsertIntoDescending(enforceUnique, _size, MaxSize, value, weight, values, weights);
            if(ret && _size < MaxSize)
                ++_size;
            return ret;
        }

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public V RemoveAt(byte index)
        {
            if(index >= _size)
                throw new ArgumentOutOfRangeException();
            var values = MemoryMarshal.CreateSpan(ref Unsafe.As<ValueArray, V>(ref Unsafe.AsRef(in _values)), _size);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<WeightArray, W>(ref Unsafe.AsRef(in _weights)), _size);
            var ret = values[index];
            SortedArrayHelper.RemoveAt(index, values, weights);
            --_size;
            return ret;
        }

        public override string ToString() => string.Join(", ", Elements.Select(x => $"{x.Value}|{x.Weight}"));
    }

}