using System;
using System.Numerics;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyTensor3DValueSemantics<T, TT>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where TT : IReadOnlyTensor3D<T>, IHaveReadOnlyContiguousSpan<T>
    {
        readonly TT _obj;
        readonly Lazy<int> _hashCode;

        public ReadOnlyTensor3DValueSemantics(TT obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hashCode = new HashCode();
                hashCode.Add(_obj.Depth);
                hashCode.Add(_obj.RowCount);
                hashCode.Add(_obj.ColumnCount);
                hashCode.Add(_obj.ReadOnlySpan);
                return hashCode.ToHashCode();
            });
        }

        public bool Equals(TT? other)
        {
            return (other is not null 
                && other.RowCount == _obj.RowCount 
                && other.Depth == _obj.Depth 
                && other.ColumnCount == _obj.ColumnCount 
                && _obj.ReadOnlySpan.SequenceEqual(other.ReadOnlySpan)
            );
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}
