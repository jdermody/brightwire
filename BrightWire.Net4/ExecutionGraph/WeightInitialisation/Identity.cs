using System;
using System.Collections.Generic;
using System.Text;

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

        public IVector CreateBias(int size)
        {
            return _lap.CreateVector(size, 0f);
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.CreateMatrix(rows, columns, (x, y) => x == y ? _value : 0f);
        }
    }
}
