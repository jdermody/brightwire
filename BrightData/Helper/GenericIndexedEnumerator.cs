using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Helper
{
    /// <summary>
    /// Generic enumerator of indexed values within a span
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public ref struct GenericIndexedEnumerator<T>
    {
        readonly ReadOnlySpan<T> _data;
        readonly ReadOnlySpan<uint> _indices;
        int _pos = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indices"></param>
        public GenericIndexedEnumerator(ReadOnlySpan<T> data, ReadOnlySpan<uint> indices)
        {
            _data = data;
            _indices = indices;
        }

        /// <summary>
        /// Current item
        /// </summary>
        public readonly ref readonly T Current => ref _data[(int)_indices[_pos]];

        /// <summary>
        /// Moves to next item
        /// </summary>
        /// <returns></returns>
        public bool MoveNext() => ++_pos < _indices.Length;

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public readonly GenericIndexedEnumerator<T> GetEnumerator() => this;
    }
}
