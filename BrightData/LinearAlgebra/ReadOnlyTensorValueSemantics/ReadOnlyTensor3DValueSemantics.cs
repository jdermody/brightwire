using System;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyTensor3DValueSemantics<T>
        where T : IReadOnlyTensor3D, IHaveReadOnlyContiguousFloatSpan
    {
        readonly T _obj;
        readonly Lazy<int> _hashCode;

        public ReadOnlyTensor3DValueSemantics(T obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hashCode = new HashCode();
                hashCode.Add(_obj.Depth);
                hashCode.Add(_obj.RowCount);
                hashCode.Add(_obj.ColumnCount);
                hashCode.Add(_obj.FloatSpan);
                return hashCode.ToHashCode();
            });
        }

        public bool Equals(T? other)
        {
            return (other is not null 
                && other.RowCount == _obj.RowCount 
                && other.Depth == _obj.Depth 
                && other.ColumnCount == _obj.ColumnCount 
                && _obj.FloatSpan.SequenceEqual(other.FloatSpan)
            );
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}
