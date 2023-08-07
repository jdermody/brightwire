using System;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics
{
    internal class ReadOnlyMatrixValueSemantics<T>
        where T : IMatrixData, IHaveReadOnlyContiguousFloatSpan
    {
        readonly T _obj;
        readonly Lazy<int> _hashCode;

        public ReadOnlyMatrixValueSemantics(T obj)
        {
            _obj = obj;
            _hashCode = new(() => {
                var hash = new HashCode();
                hash.Add(_obj.Size);
                hash.Add(_obj.FloatSpan);
                return hash.ToHashCode();
            });
        }

        public bool Equals(T? other)
        {
            return (other is not null 
                && other.RowCount == _obj.RowCount 
                && other.ColumnCount == _obj.ColumnCount 
                && _obj.FloatSpan.SequenceEqual(other.FloatSpan)
            );
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}
