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

        public ReadOnlyVectorWrapper(IReadOnlyNumericSegment<float> segment)
        {
            ReadOnlySegment = segment;
            _valueSemantics = new(this);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            var temp = SpanOwner<float>.Empty;
            ReadOnlySegment.GetSpan(ref temp, out var wasTempUsed);
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

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);
        public uint Size => ReadOnlySegment.Size;
        public bool IsReadOnly => true;
        public float this[int index] => ReadOnlySegment[index];
        public float this[uint index] => ReadOnlySegment[index];
        public float[] ToArray() => ReadOnlySegment.ToNewArray();
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(ReadOnlySegment);
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; }

        // value semantics
        public bool Equals(ReadOnlyVectorWrapper? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVectorWrapper);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Vector Wrapper ({Size}): {preview}";
        }
    }
}
