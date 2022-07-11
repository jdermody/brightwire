using System;
using System.IO;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.TensorInfo
{
    internal class MatrixInfoWrapper : IMatrixInfo
    {
        readonly TensorSegmentWrapper _segment;

        public MatrixInfoWrapper(TensorSegmentWrapper segment, uint rowCount, uint columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            _segment = segment;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            var temp = SpanOwner<float>.Empty;
            _segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                writer.Write(temp.Span.AsBytes());
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => _segment.GetSpan(ref temp, out wasTempUsed);
        public uint RowCount { get; }
        public uint ColumnCount { get; }

        public float this[int rowY, int columnX] => _segment[columnX * RowCount + rowY];
        public float this[uint rowY, uint columnX] => _segment[columnX * RowCount + rowY];

        public IMatrix Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, _segment);
        public IVectorInfo GetRow(uint rowIndex) => new VectorInfoWrapper(new TensorSegmentWrapper(_segment, rowIndex, RowCount, ColumnCount));
        public IVectorInfo GetColumn(uint columnIndex) => new VectorInfoWrapper(new TensorSegmentWrapper(_segment, columnIndex * RowCount, 1, RowCount));
        public IVectorInfo[] AllRows() => RowCount.AsRange().Select(GetRow).ToArray();
        public IVectorInfo[] AllColumns() => ColumnCount.AsRange().Select(GetColumn).ToArray();
        public uint Size => RowCount * ColumnCount;
        public ITensorSegment2 Segment => _segment;
        public override string ToString()
        {
            var preview = String.Join("|", _segment.Values.Take(Consts.PreviewSize));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Matrix Info (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
