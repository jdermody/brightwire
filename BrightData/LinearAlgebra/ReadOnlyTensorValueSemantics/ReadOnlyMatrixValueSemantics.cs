using System;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyMatrixValueSemantics<T>
        where T : IReadOnlyMatrix, IHaveReadOnlyContiguousSpan<float>
    {
        readonly T _obj;
        readonly Lazy<int> _hashCode;

        public ReadOnlyMatrixValueSemantics(T obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hash = new HashCode();
                hash.Add(_obj.Size);
                hash.Add(_obj.ReadOnlySpan);
                return hash.ToHashCode();
            });
        }

        public bool Equals(T? other)
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
