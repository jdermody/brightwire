using System;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyTensor4DValueSemantics<T>
        where T : IReadOnlyTensor4D, IHaveReadOnlyContiguousSpan<float>
    {
        readonly T _obj;
        readonly Lazy<int> _hashCode;

        public ReadOnlyTensor4DValueSemantics(T obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hashCode = new HashCode();
                hashCode.Add(_obj.Count);
                hashCode.Add(_obj.Depth);
                hashCode.Add(_obj.RowCount);
                hashCode.Add(_obj.ColumnCount);
                hashCode.Add(_obj.ReadOnlySpan);
                return hashCode.ToHashCode();
            });
        }

        public bool Equals(T? other)
        {
            return (other is not null
                && other.Count == _obj.Count
                && other.Depth == _obj.Depth
                && other.RowCount == _obj.RowCount
                && other.ColumnCount == _obj.ColumnCount
                && _obj.ReadOnlySpan.SequenceEqual(other.ReadOnlySpan)
            );
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}
