using BrightData;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Finds the single index of the highest activation and compares it to the target index
    /// </summary>
    internal class OneHotEncoding : IErrorMetric
    {
        public IMatrix<float> CalculateGradient(IMatrix<float> output, IMatrix<float> targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IReadOnlyVector<float> output, IReadOnlyVector<float> expectedOutput)
        {
            var outputIndex = output.GetMinAndMaxValues().MaxIndex;
            var expectedIndex = expectedOutput.GetMinAndMaxValues().MaxIndex;
            return outputIndex == expectedIndex ? 1f : 0f;
        }

        public float Compute(float[] output, float[] expectedOutput)
        {
            var outputIndex = output.MaximumIndex();
            var expectedIndex = expectedOutput.MaximumIndex();
            return outputIndex == expectedIndex ? 1f : 0f;
        }

        public bool DisplayAsPercentage => true;
    }
}
