using System;
using System.IO;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class Tensor4DData : ITensor4DInfo
    {
        ICanRandomlyAccessData<float> _data;
        uint _startIndex;

        public Tensor4DData(ICanRandomlyAccessData<float> data, uint startIndex, uint count, uint depth, uint rowCount, uint columnCount)
        {
            _data = data;
            _startIndex = startIndex;
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public uint Count { get; private set; }
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;
        public uint TensorSize => MatrixSize * Depth;

        public float this[int count, int depth, int rowY, int columnX] => _data[count * (int)TensorSize + depth * (int)MatrixSize + rowY * (int)ColumnCount + columnX];
        public float this[uint count, uint depth, uint rowY, uint columnX] => _data[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.GetSpan(_startIndex, Size);
        }

        public ITensor4D Create(LinearAlgebraProvider lap)
        {
            var size = Size;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size);
            segment.CopyFrom(span);
            return lap.CreateTensor4D(Count, Depth, RowCount, ColumnCount, segment);
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 4)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            Count = reader.ReadUInt32();
            _startIndex = 0;
            _data = new TempFloatData(reader, Size);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(4);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(Count);
            writer.Write(_data.GetSpan(_startIndex, Size).AsBytes());
        }

        public uint Size => Count * Depth * ColumnCount * RowCount;
    }
}
