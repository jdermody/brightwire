﻿using BrightData;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Cross entropy error
    /// https://en.wikipedia.org/wiki/Cross_entropy#Cross-entropy_error_function_and_logistic_regression
    /// </summary>
    internal class CrossEntropy : IErrorMetric
    {
        public IMatrix<float>CalculateGradient(IMatrix<float> output, IMatrix<float> targetOutput)
        {
            var lap = output.LinearAlgebraProvider;
            using var ones = lap.CreateMatrix(output.RowCount, output.ColumnCount, (_, _) => 1f);
            using var oneMinusOutput = ones.Subtract(output);
            using var oneMinusOutputTimesOutput = oneMinusOutput.PointwiseMultiply(output);
            using var delta = targetOutput.Subtract(output);
            return delta.PointwiseDivide(oneMinusOutputTimesOutput);
        }

        public float Compute(IReadOnlyVector<float> output, IReadOnlyVector<float> targetOutput)
        {
            float ret = 0;
            var len = output.Size;
            for (var i = 0; i < len; i++) {
                var a = output.ReadOnlySegment[i];
                var y = targetOutput.ReadOnlySegment[i];
                ret += Math<float>.Constrain(-y * Math<float>.Log(a) - (1.0f - y) * Math<float>.Log(1.0f - a));
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
                ret += Math<float>.Constrain(-y * Math<float>.Log(a) - (1.0f - y) * Math<float>.Log(1.0f - a));
            }
            return ret / len;
        }

        public bool DisplayAsPercentage => false;
    }
}
