using BrightWire.Models;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Finds the single index of the highest activation and compares it to the target index
    /// </summary>
    class OneHotEncoding : IErrorMetric
    {
        public IMatrix CalculateGradient(IContext context, IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(FloatVector output, FloatVector expectedOutput)
        {
            return output.MaximumIndex() == expectedOutput?.MaximumIndex() ? 1f : 0f;
        }

        public bool DisplayAsPercentage { get { return true; } }
    }
}
