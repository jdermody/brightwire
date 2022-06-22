using BrightData;
using BrightData.LinearAlegbra2;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Initalises all weights to a constant
    /// </summary>
    internal class Constant : IWeightInitialisation
    {
        readonly LinearAlgebraProvider _lap;
        readonly float _biasValue, _weightValue;

        public Constant(LinearAlgebraProvider lap, float biasValue = 0f, float weightValue = 1f)
        {
            _lap = lap;
            _biasValue = biasValue;
            _weightValue = weightValue;
        }

        public IVector CreateBias(uint size)
        {
            return _lap.CreateVector(size, _biasValue);
        }

        public IMatrix CreateWeight(uint rows, uint columns)
        {
            return _lap.CreateMatrix(rows, columns, (_, _) => _weightValue);
        }
    }
}
