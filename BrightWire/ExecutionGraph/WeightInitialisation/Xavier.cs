using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Xavier weight initialisation
    /// http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
    /// </summary>
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
            return _lap.CreateVector(size);
        }

        public IMatrix CreateWeight(int rows, int columns)
        {
            return _lap.CreateMatrix(rows, columns, (x, y) => GetWeight(rows, columns, x, y));
        }

        public float GetWeight(int inputSize, int outputSize, int i, int j)
        {
            var key = Tuple.Create(inputSize, outputSize);
            if (!_distributionTable.TryGetValue(key, out IContinuousDistribution distribution)) {
                var stdDev = _parameter / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = new Normal(0, stdDev, _random));
            }
            return Convert.ToSingle(distribution.Sample());
        }
    }
}
