using System;
using System.IO;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyMatrixWrapper : IReadOnlyMatrix
    {
        public ReadOnlyMatrixWrapper(ITensorSegment segment, uint rowCount, uint columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Segment = segment;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            var temp = SpanOwner<float>.Empty;
            Segment.GetSpan(ref temp, out var wasTempUsed);
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

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);
        public uint RowCount { get; }
        public uint ColumnCount { get; }

        public float this[int rowY, int columnX] => Segment[columnX * RowCount + rowY];
        public float this[uint rowY, uint columnX] => Segment[columnX * RowCount + rowY];

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
        public uint Size => RowCount * ColumnCount;
        public ITensorSegment Segment { get; }

        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.PreviewSize));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Matrix Info (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
