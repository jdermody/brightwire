using BrightData;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Cross entropy error
    /// https://en.wikipedia.org/wiki/Cross_entropy#Cross-entropy_error_function_and_logistic_regression
    /// </summary>
    class CrossEntropy : IErrorMetric
    {
        public IMatrix CalculateGradient(IContext context, IMatrix output, IMatrix targetOutput)
        {
            var lap = context.LinearAlgebraProvider;
            using (var ones = lap.CreateMatrix(output.RowCount, output.ColumnCount, 1f))
            using (var oneMinusOutput = ones.Subtract(output))
            using (var oneMinusOutputTimesOutput = oneMinusOutput.PointwiseMultiply(output)) {
                using (var delta = targetOutput.Subtract(output))
                    return delta.PointwiseDivide(oneMinusOutputTimesOutput);
            }
        }

        public float Compute(Vector<float> output, Vector<float> targetOutput)
        {
            float ret = 0;
            var len = output.Size;
            for (var i = 0; i < len; i++) {
                var a = output.Data[i];
                var y = targetOutput.Data[i];
                ret += BoundMath.Constrain(-y * BoundMath.Log(a) - (1.0f - y) * BoundMath.Log(1.0f - a));
            }
            return ret / len;
        }

        public bool DisplayAsPercentage => false;
    }
}
