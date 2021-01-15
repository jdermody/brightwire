using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData
{
    /// <summary>
    /// Generic vector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Vector<T> : TensorBase<T, Vector<T>>
        where T: struct
    {
        internal Vector(ITensorSegment<T> segment) : base(segment, new[] { segment.Size }) { }
        internal Vector(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        /// <summary>
        /// Number of elements in the vector
        /// </summary>
        public new uint Size => Shape[0];

        /// <summary>
        /// Creates a vector from a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment that will be used</param>
        protected override Vector<T> Create(ITensorSegment<T> segment) => new Vector<T>(segment);

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="index">Index to retrieve</param>
        /// <returns></returns>
        public T this[int index]
        {
            get => _segment[(uint)index];
            set => _segment[(uint)index] = value;
        }

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="index">Index to retrieve</param>
        public T this[uint index]
        {
            get => _segment[index];
            set => _segment[index] = value;
        }

        /// <summary>
        /// The values in the vector
        /// </summary>
        public IEnumerable<T> Values => _segment.Values;

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", _segment.Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        /// <summary>
        /// Copies from an array into this vector
        /// </summary>
        /// <param name="array">Array to copy from</param>
        public void CopyFrom(T[] array) => _segment.Initialize(array);
    }
}
