using System;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyValueSemantics<T, VT>
        where T : IHaveSize, IHaveSpanOf<VT> 
        where VT : notnull
    {
        readonly T _obj;
        readonly Lazy<int> _hashCode;

        internal ReadOnlyValueSemantics(T obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hashCode = new HashCode();
                hashCode.Add(obj.Size);
                var temp = SpanOwner<VT>.Empty;
                var span = obj.GetSpan(ref temp, out var wasTempUsed);
                try {
                    hashCode.Add(span);
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
                return hashCode.ToHashCode();
            });
        }

        public bool Equals(T? other)
        {
            if (other is not null && other.Size == _obj.Size) {
                SpanOwner<VT> temp1 = SpanOwner<VT>.Empty, temp2 = SpanOwner<VT>.Empty;
                var span1 = _obj.GetSpan(ref temp1, out var wasTemp1Used);
                var span2 = other.GetSpan(ref temp2, out var wasTemp2Used);
                try {
                    return span1.SequenceEqual(span2);
                }
                finally {
                    if(wasTemp1Used)
                        temp1.Dispose();
                    if(wasTemp2Used)
                        temp2.Dispose();
                }
            }
            return false;
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}
