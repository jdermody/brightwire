using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2.TensorInfo
{
    internal class MatrixInfoWrapper : IMatrixInfo
    {
        readonly TensorSegmentWrapper2 _segment;

        public MatrixInfoWrapper(TensorSegmentWrapper2 segment, uint rowCount, uint columnCount)
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
        public IVectorInfo GetRow(uint rowIndex) => new VectorInfoWrapper(new TensorSegmentWrapper2(_segment, rowIndex, RowCount, ColumnCount));
        public IVectorInfo GetColumn(uint columnIndex) => new VectorInfoWrapper(new TensorSegmentWrapper2(_segment, columnIndex * RowCount, 1, RowCount));
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
