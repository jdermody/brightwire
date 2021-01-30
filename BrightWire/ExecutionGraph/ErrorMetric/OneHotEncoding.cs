using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Finds the single index of the highest activation and compares it to the target index
    /// </summary>
    internal class OneHotEncoding : IErrorMetric
    {
        public IFloatMatrix CalculateGradient(IGraphContext context, IFloatMatrix output, IFloatMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(Vector<float> output, Vector<float> expectedOutput)
        {
            var outputIndex = output.MaximumIndex();
            var expectedIndex = expectedOutput?.MaximumIndex();
            return outputIndex == expectedIndex ? 1f : 0f;
        }

        public bool DisplayAsPercentage => true;
    }
}
