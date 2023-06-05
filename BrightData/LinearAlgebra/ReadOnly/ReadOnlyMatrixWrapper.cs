using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyMatrixWrapper : IReadOnlyMatrix, IEquatable<ReadOnlyMatrixWrapper>, IHaveReadOnlyContiguousFloatSpan
    {
        readonly ReadOnlyMatrixValueSemantics<ReadOnlyMatrixWrapper> _valueSemantics;

        public ReadOnlyMatrixWrapper(ITensorSegment segment, uint rowCount, uint columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Segment = segment;
            _valueSemantics = new(this);
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

        public ReadOnlySpan<float> FloatSpan => Segment.GetSpan();
        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);
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
            ? RowCount.AsRange().Select(i => Row(i).ToNewArray().ToReadOnlyVector()).ToArray()
            : RowCount.AsRange().Select(GetRow).ToArray()
        ;
        public IReadOnlyVector[] AllColumns(bool makeCopy) => makeCopy 
            ? ColumnCount.AsRange().Select(i => Column(i).ToNewArray().ToReadOnlyVector()).ToArray() 
            : ColumnCount.AsRange().Select(GetColumn).ToArray()
        ;
        public uint Size => RowCount * ColumnCount;
        public ITensorSegment Segment { get; }

        // value semantics
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyMatrixWrapper);
        public override int GetHashCode() => _valueSemantics.GetHashCode();
        public bool Equals(ReadOnlyMatrixWrapper? other) => _valueSemantics.Equals(other);

        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Matrix Wrapper (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
