namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Identity matrix: https://arxiv.org/abs/1504.00941
    /// </summary>
    class Identity : IWeightInitialisation
    {
        readonly ILinearAlgebraProvider _lap;
        readonly float _value;

        public Identity(ILinearAlgebraProvider lap, float value)
        {
            _lap = lap;
            _value = value;
        }

        public IVector CreateBias(uint size)
        {
            return _lap.CreateVector(size);
        }

        public IMatrix CreateWeight(uint rows, uint columns)
        {
            return _lap.CreateMatrix(rows, columns, (x, y) => x == y ? _value : 0f);
        }
    }
}
