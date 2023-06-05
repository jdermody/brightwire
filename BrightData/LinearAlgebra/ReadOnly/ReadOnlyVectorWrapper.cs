using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyVectorWrapper : IReadOnlyVector, IEquatable<ReadOnlyVectorWrapper>
    {
        readonly ReadOnlyVectorValueSemantics<ReadOnlyVectorWrapper> _valueSemantics;
        readonly ITensorSegment _segment;

        public ReadOnlyVectorWrapper(ITensorSegment segment)
        {
            _segment = segment;
            _valueSemantics = new(this);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            var temp = SpanOwner<float>.Empty;
            _segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                writer.Write(temp.Span.AsBytes());
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => _segment.GetSpan(ref temp, out wasTempUsed);
        public uint Size => _segment.Size;
        public float this[int index] => _segment[index];
        public float this[uint index] => _segment[index];
        public float[] ToArray() => _segment.ToNewArray();
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_segment);
        public ITensorSegment Segment => _segment;

        // value semantics
        public bool Equals(ReadOnlyVectorWrapper? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVectorWrapper);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", _segment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Vector Wrapper ({Size}): {preview}";
        }
    }
}
