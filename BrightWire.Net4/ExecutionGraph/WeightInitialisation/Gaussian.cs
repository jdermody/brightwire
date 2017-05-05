using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    class Gaussian : IWeightInitialisation
    {
        readonly IContinuousDistribution _distribution;
        readonly ILinearAlgebraProvider _lap;

        public Gaussian(ILinearAlgebraProvider lap, double stdDev = 0.1)
        {
            _lap = lap;
            _distribution = lap.IsStochastic ? new Normal(0, stdDev) : new Normal(0, stdDev, new Random(0));
        }

        float _GetBias()
        {
            return Convert.ToSingle(_distribution.Sample());
        }

        float _GetWeight(int inputSize, int outputSize, int i, int j)
        {
            var ret = Convert.ToSingle(_distribution.Sample() / Math.Sqrt(inputSize));
            return ret;
        }

        public IVector CreateBias(int size)
        {
            return _lap.Create(size, x => _GetBias());
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.Create(rows, columns, (x, y) => _GetWeight(rows, columns, x, y));
        }
    }
}
