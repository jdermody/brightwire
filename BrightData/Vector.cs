using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData
{
    public class Vector<T> : TensorBase<T, Vector<T>>
        where T: struct
    {
        public Vector(ITensorSegment<T> segment) : base(segment, new[] { segment.Size }) { }
        public Vector(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public new uint Size => Shape[0];

        protected override Vector<T> Create(ITensorSegment<T> segment) => new Vector<T>(segment);

        public T this[int index]
        {
            get => _segment[(uint)index];
            set => _segment[(uint)index] = value;
        }

        public T this[uint index]
        {
            get => _segment[index];
            set => _segment[index] = value;
        }

        public IEnumerable<T> Values => _segment.Values;

        public override string ToString()
        {
            var preview = String.Join("|", _segment.Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        public void CopyFrom(T[] array) => _segment.Initialize(array);
    }
}
