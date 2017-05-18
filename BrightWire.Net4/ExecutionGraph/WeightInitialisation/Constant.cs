using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
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

        public IVector CreateBias(int size)
        {
            return _lap.CreateVector(size, _biasValue);
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.CreateMatrix(rows, columns, _weightValue);
        }
    }
}
