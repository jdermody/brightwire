using BrightData.Helper;
using System;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    internal class QuadraticCostFunction<T>(LinearAlgebraProvider<T> lap) : ICostFunction<T> 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            using var difference = lap.Subtract(expected, predicted);
            using var squaredDifference = lap.Squared(difference);
            using var result = lap.Multiply(squaredDifference, T.CreateSaturating(0.5f));
            return Math<T>.Constrain(lap.Sum(result));
        }

        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            return lap.Subtract(expected, predicted);
        }
    }
}
