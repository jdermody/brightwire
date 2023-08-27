using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.DataTable.TensorData
{
    internal class MatrixData : IReadOnlyMatrix, IEquatable<MatrixData>, IHaveReadOnlyContiguousSpan<float>
    {
        readonly ReadOnlyMatrixValueSemantics<MatrixData> _valueSemantics;
        ICanRandomlyAccessUnmanagedData<float> _data;
        IReadOnlyNumericSegment<float>? _segment;
        uint _startIndex;

        public MatrixData(ICanRandomlyAccessUnmanagedData<float> data, uint rowCount, uint columnCount, uint startIndex)
        {
            _data = data;
            _startIndex = startIndex;
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
        }

        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public bool IsReadOnly => true;

        public float this[int rowY, int columnX]
        {
            get
            {
                _data.Get((int)(_startIndex + rowY * ColumnCount + columnX), out var ret);
                return ret;
            }
        }

        public float this[uint rowY, uint columnX]
        {
            get
            {
                _data.Get(_startIndex + rowY * ColumnCount + columnX, out var ret);
                return ret;
            }
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return FloatSpan;
        }
        public ReadOnlySpan<float> FloatSpan => _data.GetSpan(_startIndex, Size);

        public IMatrix Create(LinearAlgebraProvider lap)
        {
            var size = RowCount * ColumnCount;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size, false);
            segment.CopyFrom(span);
            return lap.CreateMatrix(RowCount, ColumnCount, segment);
        }

        public IReadOnlyVector GetRow(uint rowIndex) => new VectorData(_data, _startIndex + rowIndex, RowCount, ColumnCount);
        public IReadOnlyVector GetColumn(uint columnIndex) => new VectorData(_data, _startIndex + columnIndex * RowCount, 1, RowCount);
        public IReadOnlyVector[] AllRows() => RowCount.AsRange().Select(GetRow).ToArray();
        public IReadOnlyVector[] AllColumns() => ColumnCount.AsRange().Select(GetColumn).ToArray();

        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(_data.GetSpan(_startIndex, Size).AsBytes());
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 2)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            _startIndex = 0;
            _data = new TempFloatData(reader, Size);
        }

        public uint Size => ColumnCount * RowCount;

        // value semantics
        public bool Equals(MatrixData? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as MatrixData);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", Math.Min(Consts.DefaultPreviewSize, Size).AsRange().Select(i => {
                _data.Get(_startIndex + i, out var ret);
                return ret;
            }));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Matrix Data (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
