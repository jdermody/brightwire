using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    /// <summary>
    /// Hinge loss cost function for support vector machines (SVM) and other classification tasks.
    /// </summary>
    /// <typeparam name="T">Numeric type (typically float or double)</typeparam>
    internal class HingeLossCostFunction<T>(LinearAlgebraProvider<T> lap) : ICostFunction<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Calculates the hinge loss between predicted and expected values.
        /// </summary>
        /// <param name="predicted">Predicted values (typically -1 or 1 for binary classification)</param>
        /// <param name="expected">Expected labels (-1 or 1 for binary classification)</param>
        /// <returns>Hinge loss value</returns>
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            // Calculate margin: y * f(x) where y is expected and f(x) is prediction
            using var margin = lap.PointwiseMultiply(expected, predicted);
            
            // Calculate 1 - margin for each sample (max(0, 1 - margin))
            using var ones = lap.CreateSegment(expected.Size, T.One); 
            using var oneMinusMargin = lap.Subtract(ones, margin);
            
            // Apply max(0, value) to get hinge loss
            using var hingeloss = oneMinusMargin.ApplyReadOnlySpan(x => x.MapParallel(y => T.Max(T.Zero, y)));
            return hingeloss.Span.Average();
        }

        /// <summary>
        /// Calculates the gradient of hinge loss with respect to predicted values.
        /// </summary>
        /// <param name="predicted">Predicted values (typically -1 or 1 for binary classification)</param>
        /// <param name="expected">Expected labels (-1 or 1 for binary classification)</param>
        /// <returns>Gradient vector</returns>
        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            // Calculate margin: y * f(x) where y is expected and f(x) is prediction
            using var margin = lap.PointwiseMultiply(expected, predicted);
            
            // For hinge loss gradient calculation with respect to predictions:
            // if (margin >= 1), then gradient = 0
            // else if (margin < 1), then gradient = -y where y is expected label

            return margin.MapParallel((index, marginValue) => {
                var expectedValue = expected[index];
                if (marginValue >= T.One)
                    return T.Zero;
                else
                    return -expectedValue; // Negative of expected value for gradient computation
            });
        }
    }
}
