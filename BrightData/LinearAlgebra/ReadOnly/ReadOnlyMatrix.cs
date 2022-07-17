using System;
using System.IO;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyMatrix : IReadOnlyMatrix
    {
        float[] _data;
        ITensorSegment? _segment;

        public ReadOnlyMatrix(float[] data, uint rows, uint columns)
        {
            _data = data;
            RowCount = rows;
            ColumnCount = columns;
        }
        public ReadOnlyMatrix(uint rows, uint columns) : this(new float[rows * columns], rows, columns)
        {
        }

        public ReadOnlyMatrix(uint rows, uint columns, Func<uint, uint, float> initializer)
        {
            RowCount = rows;
            ColumnCount = columns;
            _data = new float[rows * columns];
            for (uint i = 0; i < rows; i++) {
                for (uint j = 0; j < columns; j++)
                    _data[j * rows + i] = initializer(i, j);
            }
        }

        public ITensorSegment Segment => _segment ??= new ArrayBasedTensorSegment(_data);

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(_data.AsSpan().AsBytes());
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
            return _data.AsSpan();
        }

        public uint Size => RowCount * ColumnCount;
        public uint RowCount { get; private set; }
        public uint ColumnCount { get;private set; }
        public float this[int rowY, int columnX] => _data[columnX * RowCount + rowY];
        public float this[uint rowY, uint columnX] => _data[columnX * RowCount + rowY];
        public IMatrix Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, Segment);
        public TensorSegmentWrapper Row(uint index) => new(Segment, index, RowCount, ColumnCount);
        public TensorSegmentWrapper Column(uint index) => new(Segment, index * RowCount, 1, RowCount);
        public IReadOnlyVector GetRow(uint rowIndex) => new ReadOnlyVectorWrapper(Row(rowIndex));
        public IReadOnlyVector GetColumn(uint columnIndex) => new ReadOnlyVectorWrapper(Column(columnIndex));
        public IReadOnlyVector[] AllRows(bool makeCopy) => makeCopy
            ? RowCount.AsRange().Select(i => Row(i).ToNewArray().ToVectorInfo()).ToArray()
            : RowCount.AsRange().Select(GetRow).ToArray()
        ;
        public IReadOnlyVector[] AllColumns(bool makeCopy) => makeCopy
            ? ColumnCount.AsRange().Select(i => Column(i).ToNewArray().ToVectorInfo()).ToArray()
            : ColumnCount.AsRange().Select(GetColumn).ToArray()
        ;
        public override string ToString()
        {
            var preview = String.Join("|", _data.Take(Consts.PreviewSize));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Matrix Info (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
