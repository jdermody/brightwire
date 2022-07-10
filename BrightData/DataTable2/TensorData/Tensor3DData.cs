using System;
using System.IO;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class Tensor3DData : ITensor3DInfo
    {
        ICanRandomlyAccessUnmanagedData<float> _data;
        ITensorSegment2? _segment;
        uint _startIndex;

        public Tensor3DData(ICanRandomlyAccessUnmanagedData<float> data, uint depth, uint rowCount, uint columnCount, uint startIndex)
        {
            _data = data;
            _startIndex = startIndex;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;

        public float this[int depth, int rowY, int columnX]
        {
            get
            {
                _data.Get((int)(_startIndex + depth * MatrixSize + rowY * (int)ColumnCount + columnX), out var ret);
                return ret;
            }
        }

        public float this[uint depth, uint rowY, uint columnX]
        {
            get
            {
                _data.Get(_startIndex + depth * MatrixSize + rowY * ColumnCount + columnX, out var ret);
                return ret;
            }
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.GetSpan(_startIndex, Size);
        }

        public ITensor3D Create(LinearAlgebraProvider lap)
        {
            var size = Size;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size);
            segment.CopyFrom(span);
            return lap.CreateTensor3D(Depth, RowCount, ColumnCount, segment);
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 3)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            _startIndex = 0;
            _data = new TempFloatData(reader, Size);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(_data.GetSpan(_startIndex, Size).AsBytes());
        }

        public uint Size => Depth * ColumnCount * RowCount;
        public ITensorSegment2 Segment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());
    }
}
