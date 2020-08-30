namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Initalises all weights to a constant
    /// </summary>
    class Constant : IWeightInitialisation
    {
        readonly ILinearAlgebraProvider _lap;
        readonly float _biasValue, _weightValue;

        public Constant(ILinearAlgebraProvider lap, float biasValue = 0f, float weightValue = 1f)
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
            return _lap.CreateMatrix(rows, columns, _weightValue);
        }
    }
}
