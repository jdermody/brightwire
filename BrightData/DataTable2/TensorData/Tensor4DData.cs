using System;
using System.IO;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class Tensor4DData : ITensor4DInfo
    {
        readonly ICanRandomlyAccessData<float> _data;
        readonly uint _startIndex;

        public Tensor4DData(ICanRandomlyAccessData<float> data, uint startIndex, uint count, uint depth, uint rowCount, uint columnCount)
        {
            _data = data;
            _startIndex = startIndex;
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public uint Count { get; internal init; }
        public uint Depth { get; internal init; }
        public uint RowCount { get; internal init; }
        public uint ColumnCount { get; internal init; }
        public uint MatrixSize => RowCount * ColumnCount;
        public uint TensorSize => MatrixSize * Depth;

        public float this[int count, int depth, int rowY, int columnX] => _data[count * (int)TensorSize + depth * (int)MatrixSize + rowY * (int)ColumnCount + columnX];
        public float this[uint count, uint depth, uint rowY, uint columnX] => _data[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            var size = RowCount * ColumnCount * Depth * Count;
            return _data.GetSpan(_startIndex, size);
        }

        public ITensor4D Create(LinearAlgebraProvider lap)
        {
            var size = RowCount * ColumnCount * Depth * Count;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size);
            segment.CopyFrom(span);
            return lap.CreateTensor4D(Count, Depth, RowCount, ColumnCount, segment);
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void WriteTo(BinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
