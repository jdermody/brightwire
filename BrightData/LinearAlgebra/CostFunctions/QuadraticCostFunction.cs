using BrightData.Helper;
using System;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    internal class QuadraticCostFunction(LinearAlgebraProvider lap) : ICostFunction<float>
    {
        public float Cost(IReadOnlyNumericSegment<float> predicted, IReadOnlyNumericSegment<float> expected)
        {
            using var difference = lap.Subtract(expected, predicted);
            using var squaredDifference = lap.Squared(difference);
            using var result = lap.Multiply(squaredDifference, 0.5f);
            return FloatMath.Constrain(lap.Sum(result));
        }

        public IReadOnlyNumericSegment<float> Gradient(IReadOnlyNumericSegment<float> predicted, IReadOnlyNumericSegment<float> expected)
        {
            return lap.Subtract(expected, predicted);
        }
    }
}
