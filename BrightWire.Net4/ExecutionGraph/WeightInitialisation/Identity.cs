using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
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
            return _lap.Create(size, 0f);
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.Create(rows, columns, (x, y) => x == y ? _value : 0f);
        }
    }
}
