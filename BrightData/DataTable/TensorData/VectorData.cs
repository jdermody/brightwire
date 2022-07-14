using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable.TensorData
{
    internal class VectorData : IVectorInfo
    {
        ICanRandomlyAccessUnmanagedData<float> _data;
        ITensorSegment? _segment;
        uint _startIndex;
        uint _stride;

        internal VectorData(ICanRandomlyAccessUnmanagedData<float> data, uint startIndex, uint stride, uint size)
        {
            _data = data;
            _startIndex = startIndex;
            _stride = stride;
            Size = size;
        }

        public uint Size { get; private set; }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            if (_stride == 1) {
                wasTempUsed = false;
                return _data.GetSpan(_startIndex, Size);
            }
            temp = SpanOwner<float>.Allocate((int)Size);
            wasTempUsed = true;
            var ret = temp.Span;
            for(var i = 0; i < Size; i++)
                ret[i] = this[i];
            return ret;
        }

        public IVector Create(LinearAlgebraProvider lap)
        {
            var span = _data.GetSpan(_startIndex, Size);
            var segment = lap.CreateSegment(Size);
            segment.CopyFrom(span);
            return lap.CreateVector(segment);
        }

        public ITensorSegment Segment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());

        public float this[int index]
        {
            get
            {
                _data.Get((int)(_startIndex + index * _stride), out var ret);
                return ret;
            }
        }
        public float this[uint index]
        {
            get
            {
                _data.Get(_startIndex + index * _stride, out var ret);
                return ret;
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new Exception("Unexpected array size");
            Size = reader.ReadUInt32();
            _startIndex = 0;
            _stride = 1;
            _data = new TempFloatData(reader, Size);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(_data.GetSpan(_startIndex, Size).AsBytes());
        }
        public override string ToString()
        {
            var preview = String.Join("|", Math.Min(Consts.PreviewSize, Size).AsRange().Select(i => this[i]));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Vector Data ({Size}): {preview}";
        }
    }
}
