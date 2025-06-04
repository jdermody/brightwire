using BrightData.Helper;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    internal class MeanSquaredErrorCostFunction<T>(LinearAlgebraProvider<T> lap) : ICostFunction<T> 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            using var difference        = lap.Subtract(expected, predicted);
            using var squaredDifference = lap.Squared(difference);
            using var result            = lap.Multiply(squaredDifference, T.CreateSaturating(0.5f));
            return Math<T>.Constrain(lap.Sum(result));
        }

        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            using var ret = lap.Subtract(expected, predicted);
            return lap.Multiply(ret, T.One / T.CreateSaturating(expected.Size));
        }
    }
}
