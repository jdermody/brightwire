using BrightData.Helper;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    /// <summary>
    /// Huber loss cost function for robust regression that combines MSE and MAE.
    /// </summary>
    /// <typeparam name="T">Numeric type (typically float or double)</typeparam>
    /// <remarks>
    /// Creates a new Huber loss cost function with specified delta parameter.
    /// </remarks>
    /// <param name="lap">Linear algebra provider</param>
    /// <param name="delta">Delta parameter for the huber loss (default is 1.0)</param>
    internal class HuberLossCostFunction<T>(LinearAlgebraProvider<T> lap, T? delta) : ICostFunction<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        private readonly LinearAlgebraProvider<T> _lap = lap;
        private readonly T _delta = delta ?? T.CreateSaturating(1.0f);

        /// <summary>
        /// Calculates the huber loss between predicted and expected values.
        /// </summary>
        /// <param name="predicted">Predicted values</param>
        /// <param name="expected">Expected values</param>
        /// <returns>Huber loss value</returns>
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            // Calculate the difference between prediction and actual
            using var difference = _lap.Subtract(expected, predicted);
            
            // Calculate absolute differences for each element
            using var absDifference = _lap.Abs(difference);

            // Calculate huber loss per element: 0.5 * x^2 if |x| <= delta else delta * (|x| - 0.5 * delta)
            using var ret = absDifference.MapParallel((index, absDiff) => {
                var diff = difference[index];
                if (absDiff <= _delta)
                    // Quadratic loss for small errors: 0.5 * difference^2
                    return T.CreateSaturating(0.5f) * diff * diff;
                else
                    // Linear loss for large errors: delta * (|difference| - 0.5 * delta)
                    return _delta * (absDiff - T.CreateSaturating(0.5f) * _delta);
            });

            // Return average loss
            return ret.ApplyReadOnlySpan(x => x.Average());
        }

        /// <summary>
        /// Calculates the gradient of huber loss with respect to predicted values.
        /// </summary>
        /// <param name="predicted">Predicted values</param>
        /// <param name="expected">Expected values</param>
        /// <returns>Gradient vector</returns>
        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            // Calculate the difference between prediction and actual
            using var difference = _lap.Subtract(expected, predicted);
            
            // Return gradient: -difference if |difference| <= delta else -sign(difference) * delta
            return difference.MapParallel((index, diff) => {
                var absDiff = T.Abs(diff); // Calculate absolute value directly from diff
                if (absDiff <= _delta)
                    return -diff;  // Gradient is -difference for small errors
                else
                    return T.CreateSaturating(T.Sign(diff)) * -_delta;  // Gradient is -sign(difference) * delta for large errors
            });
        }
    }
}
