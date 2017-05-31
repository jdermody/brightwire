using MathNet.Numerics.Distributions;
using System;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Initialises weights randomly based on a gaussian distribution
    /// </summary>
    class Gaussian : IWeightInitialisation
    {
        readonly IContinuousDistribution _distribution;
        readonly ILinearAlgebraProvider _lap;
        readonly bool _zeroBias;

        public Gaussian(ILinearAlgebraProvider lap, bool zeroInitialBias = true, double stdDev = 0.1)
        {
            _lap = lap;
            _zeroBias = zeroInitialBias;
            _distribution = lap.IsStochastic ? new Normal(0, stdDev) : new Normal(0, stdDev, new Random(0));
        }

        float _GetBias()
        {
            return _zeroBias ? 0f : Convert.ToSingle(_distribution.Sample());
        }

        float _GetWeight(int inputSize, int outputSize, int i, int j)
        {
            var ret = Convert.ToSingle(_distribution.Sample() / Math.Sqrt(inputSize));
            return ret;
        }

        public IVector CreateBias(int size)
        {
            return _lap.CreateVector(size, x => _GetBias());
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.CreateMatrix(rows, columns, (x, y) => _GetWeight(rows, columns, x, y));
        }
    }
}
