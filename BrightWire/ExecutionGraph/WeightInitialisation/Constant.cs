using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Initalises all weights to a constant
    /// </summary>
    internal class Constant(LinearAlgebraProvider lap, float biasValue = 0f, float weightValue = 1f)
        : IWeightInitialisation
    {
        public IVector CreateBias(uint size)
        {
            return lap.CreateVector(size, biasValue);
        }

        public IMatrix CreateWeight(uint rows, uint columns)
        {
            return lap.CreateMatrix(rows, columns, (_, _) => weightValue);
        }
    }
}
