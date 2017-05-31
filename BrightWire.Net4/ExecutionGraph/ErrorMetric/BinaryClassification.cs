using BrightWire.Models;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Binary classification rounds outputs to 1 or 0 and compares them against the target
    /// </summary>
    class BinaryClassification : IErrorMetric
    {
        public IMatrix CalculateGradient(IContext context, IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(FloatVector output, FloatVector targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Size; i++) {
                var val = (output.Data[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (val == targetOutput.Data[i]) ? 1.0f : 0.0f;
            }
            return ret / output.Size;
        }

        public bool DisplayAsPercentage { get { return true; } }
    }
}
