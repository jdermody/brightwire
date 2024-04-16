using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Identity matrix: https://arxiv.org/abs/1504.00941
    /// </summary>
    internal class Identity(LinearAlgebraProvider<float> lap, float value) : IWeightInitialisation
    {
        public IVector<float> CreateBias(uint size)
        {
            return lap.CreateVector(size, true);
        }

        public IMatrix<float> CreateWeight(uint rows, uint columns)
        {
            return lap.CreateMatrix(rows, columns, (x, y) => x == y ? value : 0f);
        }
    }
}
