using System;
using System.IO;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class Tensor3DData : ITensor3DInfo
    {
        readonly ICanRandomlyAccessData<float> _data;
        readonly uint _startIndex;

        public Tensor3DData(BrightDataContext context, ICanRandomlyAccessData<float> data, uint startIndex, uint depth, uint rowCount, uint columnCount)
        {
            _data = data;
            _startIndex = startIndex;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public uint Depth { get; internal init; }
        public uint RowCount { get; internal init; }
        public uint ColumnCount { get; internal init; }
        public uint MatrixSize => RowCount * ColumnCount;

        public float this[int depth, int rowY, int columnX]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float this[uint depth, uint rowY, uint columnX]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            var size = Depth * RowCount * ColumnCount;
            return _data.GetSpan(_startIndex, size);
        }

        public ITensor3D Create(LinearAlgebraProvider lap)
        {
            var size = Depth * RowCount * ColumnCount;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size);
            segment.CopyFrom(span);
            return lap.CreateTensor3D(Depth, RowCount, ColumnCount, segment);
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
