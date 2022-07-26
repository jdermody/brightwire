using System;
using System.IO;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable.TensorData
{
    internal class Tensor4DData : IReadOnlyTensor4D
    {
        ICanRandomlyAccessUnmanagedData<float> _data;
        ITensorSegment? _segment;
        uint _startIndex;

        public Tensor4DData(ICanRandomlyAccessUnmanagedData<float> data, uint count, uint depth, uint rowCount, uint columnCount, uint startIndex)
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

        public float this[int count, int depth, int rowY, int columnX]
        {
            get
            {
                _data.Get((int)(_startIndex + count * TensorSize + depth * (int)MatrixSize + rowY * (int)ColumnCount + columnX), out var ret);
                return ret;
            }
        }

        public float this[uint count, uint depth, uint rowY, uint columnX]
        {
            get
            {
                _data.Get(_startIndex + count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX, out var ret);
                return ret;
            }
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.GetSpan(_startIndex, Size);
        }

        public ITensor4D Create(LinearAlgebraProvider lap)
        {
            var size = Size;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size, false);
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
        public ITensorSegment Segment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());
    }
}
