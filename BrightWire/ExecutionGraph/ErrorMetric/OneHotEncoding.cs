using BrightData;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Finds the single index of the highest activation and compares it to the target index
    /// </summary>
    class OneHotEncoding : IErrorMetric
    {
        public IFloatMatrix CalculateGradient(IContext context, IFloatMatrix output, IFloatMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(Vector<float> output, Vector<float> expectedOutput)
        {
            return output.MaximumIndex() == expectedOutput?.MaximumIndex() ? 1f : 0f;
        }

        public bool DisplayAsPercentage => true;
    }
}
