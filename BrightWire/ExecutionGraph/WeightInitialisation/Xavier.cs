using System;
using System.Collections.Generic;
using BrightData;
using BrightData.Distribution;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Xavier weight initialisation
    /// http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
    /// </summary>
    internal class Xavier : IWeightInitialisation
    {
        readonly float _parameter;
        readonly ILinearAlgebraProvider _lap;
        readonly Dictionary<(uint, uint), IContinuousDistribution> _distributionTable = new Dictionary<(uint, uint), IContinuousDistribution>();

        public Xavier(ILinearAlgebraProvider lap, float parameter = 6)
        {
            _lap = lap;
            _parameter = MathF.Sqrt(parameter);
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
            if (!_distributionTable.TryGetValue(key, out var distribution)) {
                var stdDev = _parameter / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = _lap.Context.CreateContinuousDistribution(0, stdDev));
            }
            return Convert.ToSingle(distribution.Sample());
        }
    }
}
