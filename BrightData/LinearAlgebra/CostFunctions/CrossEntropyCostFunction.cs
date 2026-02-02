using BrightData.Helper;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    /// <summary>
    /// Cross-entropy cost function for binary classification problems.
    /// </summary>
    /// <typeparam name="T">Numeric type (typically float or double)</typeparam>
    internal class CrossEntropyCostFunction<T>(LinearAlgebraProvider<T> lap) : ICostFunction<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Calculates the cross-entropy loss between predicted and expected values.
        /// </summary>
        /// <param name="predicted">Predicted probability values</param>
        /// <param name="expected">Expected binary labels (0 or 1)</param>
        /// <returns>Cross-entropy loss value</returns>
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            // Create reusable segments to reduce allocations
            var ones = lap.CreateSegment(expected.Size, T.One);
            try
            {
                using var oneMinusExpected  = lap.Subtract(ones, expected);
                using var oneMinusPredicted = lap.Subtract(ones, predicted);
                lap.AddInPlace(oneMinusPredicted, Math<T>.AlmostZero);

                using var logOneMinusPredicted = lap.Log(oneMinusPredicted);
                using var predictedEpsilon = lap.Add(predicted, Math<T>.AlmostZero);
                using var logPredicted = lap.Log(predictedEpsilon);
                using var expectedTimesLogPredicted = lap.PointwiseMultiply(expected, logPredicted);
                using var oneMinusExpectedTimesLogOneMinusPredicted = lap.PointwiseMultiply(oneMinusExpected, logOneMinusPredicted);
                using var result = lap.Add(expectedTimesLogPredicted, oneMinusExpectedTimesLogOneMinusPredicted);
                var ret = lap.Average(result);
                return ret * T.NegativeOne;
            }
            finally
            {
                ones.Dispose();
            }
        }

        /// <summary>
        /// Calculates the gradient of cross-entropy loss with respect to predicted values.
        /// </summary>
        /// <param name="predicted">Predicted probability values</param>
        /// <param name="expected">Expected binary labels (0 or 1)</param>
        /// <returns>Gradient vector</returns>
        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected) => lap.Subtract(expected, predicted);
    }
}
