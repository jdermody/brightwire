using System;
using System.IO;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.TensorInfo
{
    internal class MatrixInfo : IMatrixInfo
    {
        float[] _data;
        ITensorSegment2? _segment;

        public MatrixInfo(float[] data, uint rows, uint columns)
        {
            _data = data;
            RowCount = rows;
            ColumnCount = columns;
        }
        public MatrixInfo(uint rows, uint columns) : this(new float[rows * columns], rows, columns)
        {
        }

        public MatrixInfo(uint rows, uint columns, Func<uint, uint, float> initializer)
        {
            RowCount = rows;
            ColumnCount = columns;
            _data = new float[rows * columns];
            for (uint i = 0; i < rows; i++) {
                for (uint j = 0; j < columns; j++)
                    _data[j * rows + i] = initializer(i, j);
            }
        }

        public ITensorSegment2 Segment => _segment ??= new ArrayBasedTensorSegment(_data);

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
        public IVectorInfo GetRow(uint rowIndex) => new VectorInfoWrapper(new TensorSegmentWrapper(Segment, rowIndex, RowCount, ColumnCount));
        public IVectorInfo GetColumn(uint columnIndex) => new VectorInfoWrapper(new TensorSegmentWrapper(Segment, columnIndex * RowCount, 1, RowCount));
        public IVectorInfo[] AllRows() => RowCount.AsRange().Select(GetRow).ToArray();
        public IVectorInfo[] AllColumns() => ColumnCount.AsRange().Select(GetColumn).ToArray();
        public override string ToString()
        {
            var preview = String.Join("|", _data.Take(Consts.PreviewSize));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Matrix Info (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
