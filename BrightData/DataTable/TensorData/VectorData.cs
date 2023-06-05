using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.DataTable.TensorData
{
    internal class VectorData : IReadOnlyVector, IEquatable<VectorData>
    {
        readonly ReadOnlyVectorValueSemantics<VectorData> _valueSemantics;
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
            _valueSemantics = new(this);
        }

        public uint Size { get; private set; }

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
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
            var segment = lap.CreateSegment(Size, false);
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

        // value semantics
        public bool Equals(VectorData? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as VectorData);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", Math.Min(Consts.DefaultPreviewSize, Size).AsRange().Select(i => this[i]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector Data ({Size}): {preview}";
        }
    }
}
