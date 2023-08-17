using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyMatrix : IReadOnlyMatrix, IEquatable<ReadOnlyMatrix>, IHaveReadOnlyContiguousSpan<float>
    {
        readonly ReadOnlyMatrixValueSemantics<ReadOnlyMatrix> _valueSemantics;
        float[] _data;
        INumericSegment<float>? _segment;

#pragma warning disable CS8618
        ReadOnlyMatrix()
#pragma warning restore CS8618
        {
            _valueSemantics = new(this);
        }

        public ReadOnlyMatrix(float[] data, uint rows, uint columns) : this()
        {
            _data = data;
            RowCount = rows;
            ColumnCount = columns;
        }
        public ReadOnlyMatrix(uint rows, uint columns) : this(new float[rows * columns], rows, columns)
        {
        }

        public unsafe ReadOnlyMatrix(uint rows, uint columns, Func<uint, uint, float> initializer) : this()
        {
            RowCount = rows;
            ColumnCount = columns;
            _data = new float[rows * columns];
            fixed (float* ptr = &_data[0]) {
                var p = ptr;
                for (uint i = 0, len = (uint)_data.Length; i < len; i++)
                    *p++ = initializer(i % rows, i / rows);
            }
        }

        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment ??= new ArrayBasedTensorSegment(_data);

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(FloatSpan.AsBytes());
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 2)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            _data = reader.BaseStream.ReadArray<float>(Size);
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return FloatSpan;
        }
        public ReadOnlySpan<float> FloatSpan => _data.AsSpan();

        public uint Size => RowCount * ColumnCount;
        public uint RowCount { get; private set; }
        public uint ColumnCount { get;private set; }
        public bool IsReadOnly => true;
        public float this[int rowY, int columnX] => _data[columnX * RowCount + rowY];
        public float this[uint rowY, uint columnX] => _data[columnX * RowCount + rowY];
        public IMatrix Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, ReadOnlySegment);
        public TensorSegmentWrapper Row(uint index) => new((INumericSegment<float>)ReadOnlySegment, index, RowCount, ColumnCount);
        public TensorSegmentWrapper Column(uint index) => new((INumericSegment<float>)ReadOnlySegment, index * RowCount, 1, RowCount);
        public IReadOnlyVector GetRow(uint rowIndex) => new ReadOnlyVectorWrapper(Row(rowIndex));
        public IReadOnlyVector GetColumn(uint columnIndex) => new ReadOnlyVectorWrapper(Column(columnIndex));
        public IReadOnlyVector[] AllRows() => RowCount.AsRange().Select(GetRow).ToArray();
        public IReadOnlyVector[] AllColumns() => ColumnCount.AsRange().Select(GetColumn).ToArray();

        // value semantics
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyMatrix);
        public bool Equals(ReadOnlyMatrix? other) => _valueSemantics.Equals(other);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", _data.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
