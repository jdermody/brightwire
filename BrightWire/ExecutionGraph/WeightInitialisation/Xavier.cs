using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    class Xavier : IWeightInitialisation
    {
        readonly double _parameter;
        readonly ILinearAlgebraProvider _lap;
        readonly Dictionary<Tuple<int, int>, IContinuousDistribution> _distributionTable = new Dictionary<Tuple<int, int>, IContinuousDistribution>();
        readonly Random _random;

        public Xavier(ILinearAlgebraProvider lap, float parameter = 6)
        {
            _lap = lap;
            _parameter = Math.Sqrt(parameter);
            _random = lap.IsStochastic ? new Random() : new Random(0);
        }

        public IVector CreateBias(int size)
        {
            return _lap.Create(size, 0f);
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.Create(rows, columns, (x, y) => GetWeight(rows, columns, x, y));
        }

        public float GetWeight(int inputSize, int outputSize, int i, int j)
        {
            var key = Tuple.Create(inputSize, outputSize);
            IContinuousDistribution distribution;
            if (!_distributionTable.TryGetValue(key, out distribution)) {
                var stdDev = _parameter / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = new Normal(0, stdDev, _random));
            }
            return Convert.ToSingle(distribution.Sample());
        }
    }
}
