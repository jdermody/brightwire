using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData
{
    public class Vector<T> : TensorBase<T, Vector<T>>, IIndexableVector<T>
        where T: struct
    {
        public Vector(ITensorSegment<T> data) : base(data.Context, data, new[] { data.Size }) { }
        public Vector(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public new uint Size => Shape[0];

        IVector<T> IVector<T>.Sigmoid() => Sigmoid();

        public IVector<T> Subtract(IVector<T> vector) => base.Subtract((Vector<T>) vector);
        public void AddInPlace(IVector<T> vector) => base.AddInPlace((Vector<T>)vector);

        IVector<T> IVector<T>.Log() => Log();

        public IVector<T> Clone()
        {
            return Context.CreateVector(Size, i => this[i]);
        }

        public T DotProduct(IVector<T> vector) => base.DotProduct((Vector<T>)vector);

        IMatrix<T> IVector<T>.Reshape(uint rows, uint columns) => base.Reshape(rows, columns);

        public IIndexableVector<T> AsIndexable() => this;

        protected override Vector<T> Create(ITensorSegment<T> segment) => new Vector<T>(segment);

        public T this[int index]
        {
            get => _data[(uint)index];
            set => _data[(uint)index] = value;
        }

        public T this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public IEnumerable<T> Values => _data.Values;

        public override string ToString()
        {
            var preview = String.Join("|", _data.Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }
    }
}
