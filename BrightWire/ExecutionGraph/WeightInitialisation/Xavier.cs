using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using BrightData;

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
        readonly Dictionary<(uint, uint), IContinuousDistribution> _distributionTable = new Dictionary<(uint, uint), IContinuousDistribution>();
        readonly Random _random;

        public Xavier(ILinearAlgebraProvider lap, float parameter = 6)
        {
            _lap = lap;
            _parameter = Math.Sqrt(parameter);
            _random = lap.Context.Random;
        }

        public IFloatVector CreateBias(uint size)
        {
            return _lap.CreateVector(size);
        }

        public IFloatMatrix CreateWeight(uint rows, uint columns)
        {
            return _lap.CreateMatrix(rows, columns, (x, y) => GetWeight(rows, columns));
        }

        public float GetWeight(uint inputSize, uint outputSize)
        {
            var key = (inputSize, outputSize);
            if (!_distributionTable.TryGetValue(key, out IContinuousDistribution distribution)) {
                var stdDev = _parameter / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = new Normal(0, stdDev, _random));
            }
            return Convert.ToSingle(distribution.Sample());
        }
    }
}
