using BrightData;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Cross entropy error
    /// https://en.wikipedia.org/wiki/Cross_entropy#Cross-entropy_error_function_and_logistic_regression
    /// </summary>
    internal class CrossEntropy : IErrorMetric
    {
        public IMatrix CalculateGradient(IGraphContext context, IMatrix output, IMatrix targetOutput)
        {
            var lap = context.GetLinearAlgebraProvider();
            using var ones = lap.CreateMatrix(output.RowCount, output.ColumnCount, (i, j) => 1f);
            using var oneMinusOutput = ones.Subtract(output);
            using var oneMinusOutputTimesOutput = oneMinusOutput.PointwiseMultiply(output);
            using var delta = targetOutput.Subtract(output);
            return delta.PointwiseDivide(oneMinusOutputTimesOutput);
        }

        public float Compute(IVectorData output, IVectorData targetOutput)
        {
            float ret = 0;
            var len = output.Size;
            for (var i = 0; i < len; i++) {
                var a = output.ReadOnlySegment[i];
                var y = targetOutput.ReadOnlySegment[i];
                ret += FloatMath.Constrain(-y * FloatMath.Log(a) - (1.0f - y) * FloatMath.Log(1.0f - a));
            }
            return ret / len;
        }

        public float Compute(float[] output, float[] targetOutput)
        {
            float ret = 0;
            var len = output.Length;
            for (var i = 0; i < len; i++) {
                var a = output[i];
                var y = targetOutput[i];
                ret += FloatMath.Constrain(-y * FloatMath.Log(a) - (1.0f - y) * FloatMath.Log(1.0f - a));
            }
            return ret / len;
        }

        public bool DisplayAsPercentage => false;
    }
}
