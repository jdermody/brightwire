using System;
using System.Numerics;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyMatrixValueSemantics<T, TT>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where TT : IReadOnlyMatrix<T>, IHaveReadOnlyContiguousMemory<T>
    {
        readonly TT _obj;
        readonly Lazy<int> _hashCode;

        public ReadOnlyMatrixValueSemantics(TT obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hash = new HashCode();
                hash.Add(_obj.Size);
                hash.Add(_obj.ReadOnlySpan);
                return hash.ToHashCode();
            });
        }

        public bool Equals(TT? other)
        {
            return (other is not null 
                && other.RowCount == _obj.RowCount 
                && other.ColumnCount == _obj.ColumnCount 
                && _obj.ReadOnlySpan.SequenceEqual(other.ReadOnlySpan)
            );
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}
