using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Initializes all weights to a constant
    /// </summary>
    internal class Constant(LinearAlgebraProvider<float> lap, float biasValue = 0f, float weightValue = 1f)
        : IWeightInitialisation
    {
        public IVector<float> CreateBias(uint size)
        {
            return lap.CreateVector(size, biasValue);
        }

        public IMatrix<float> CreateWeight(uint rows, uint columns)
        {
            return lap.CreateMatrix(rows, columns, (_, _) => weightValue);
        }
    }
}
