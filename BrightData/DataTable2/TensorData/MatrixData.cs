using System;
using System.IO;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class MatrixData : IMatrixInfo
    {
        ICanRandomlyAccessData<float> _data;
        ITensorSegment2? _segment;
        uint _startIndex;

        public MatrixData(ICanRandomlyAccessData<float> data, uint rowCount, uint columnCount, uint startIndex)
        {
            _data = data;
            _startIndex = startIndex;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }

        public float this[int rowY, int columnX] => _data[rowY * (int)ColumnCount + columnX];
        public float this[uint rowY, uint columnX] => _data[rowY * ColumnCount + columnX];

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.GetSpan(_startIndex, Size);
        }

        public IMatrix Create(LinearAlgebraProvider lap)
        {
            var size = RowCount * ColumnCount;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size);
            segment.CopyFrom(span);
            return lap.CreateMatrix(RowCount, ColumnCount, segment);
        }

        public IVectorInfo GetRow(uint rowIndex) => new VectorData(_data, rowIndex, RowCount, ColumnCount);
        public IVectorInfo GetColumn(uint columnIndex) => new VectorData(_data, columnIndex * RowCount, 1, RowCount);
        public ITensorSegment2 Segment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());

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
    }
}
