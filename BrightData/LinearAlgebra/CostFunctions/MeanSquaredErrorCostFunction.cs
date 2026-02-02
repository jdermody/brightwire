using BrightData.Helper;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    /// <summary>
    /// Mean squared error cost function for regression problems.
    /// </summary>
    /// <typeparam name="T">Numeric type (typically float or double)</typeparam>
    internal class MeanSquaredErrorCostFunction<T>(LinearAlgebraProvider<T> lap) : ICostFunction<T> 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Calculates the mean squared error between predicted and expected values.
        /// </summary>
        /// <param name="predicted">Predicted values</param>
        /// <param name="expected">Expected values</param>
        /// <returns>Mean squared error value</returns>
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            // Calculate difference and square it in a more efficient manner
            using var difference = lap.Subtract(expected, predicted);
            using var squaredDifference = lap.Squared(difference);
            using var result = lap.Multiply(squaredDifference, T.CreateSaturating(0.5f));
            return Math<T>.Constrain(lap.Sum(result));
        }

        /// <summary>
        /// Calculates the gradient of mean squared error with respect to predicted values.
        /// </summary>
        /// <param name="predicted">Predicted values</param>
        /// <param name="expected">Expected values</param>
        /// <returns>Gradient vector</returns>
        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            using var ret = lap.Subtract(expected, predicted);
            // Optimization: use the reciprocal of size directly rather than division
            return lap.Multiply(ret, T.One / T.CreateSaturating(expected.Size));
        }
    }
}
