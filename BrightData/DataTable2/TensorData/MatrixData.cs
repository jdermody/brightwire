using System;
using System.IO;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class MatrixData : IMatrixInfo
    {
        readonly ICanRandomlyAccessData<float> _data;
        readonly uint _startIndex;

        public MatrixData(ICanRandomlyAccessData<float> data, uint startIndex, uint rowCount, uint columnCount)
        {
            _data = data;
            _startIndex = startIndex;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public uint RowCount { get; }
        public uint ColumnCount { get; }

        public float this[int rowY, int columnX] => _data[rowY * (int)ColumnCount + columnX];
        public float this[uint rowY, uint columnX] => _data[rowY * ColumnCount + columnX];

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            var size = RowCount * ColumnCount;
            return _data.GetSpan(_startIndex, size);
        }

        public IMatrix Create(LinearAlgebraProvider lap)
        {
            var size = RowCount * ColumnCount;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size);
            segment.CopyFrom(span);
            return lap.CreateMatrix(RowCount, ColumnCount, segment);
        }

        public IVectorInfo GetRow(uint rowIndex) => new VectorData(_data, rowIndex * ColumnCount, RowCount);

        public void WriteTo(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
