using System;
using System.Collections.Generic;
using BrightData;
using BrightData.LinearAlegbra2;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Xavier weight initialisation
    /// http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
    /// </summary>
    internal class Xavier : IWeightInitialisation
    {
        readonly float _parameter;
        readonly LinearAlgebraProvider _lap;
        readonly Dictionary<(uint, uint), IContinuousDistribution> _distributionTable = new();

        public Xavier(LinearAlgebraProvider lap, float parameter = 6)
        {
            _lap = lap;
            _parameter = MathF.Sqrt(parameter);
        }

        public IVector CreateBias(uint size)
        {
            return _lap.CreateVector(size);
        }

        public IMatrix CreateWeight(uint rows, uint columns)
        {
            return _lap.CreateMatrix(rows, columns, (_, _) => GetWeight(rows, columns));
        }

        public float GetWeight(uint inputSize, uint outputSize)
        {
            var key = (inputSize, outputSize);
            if (!_distributionTable.TryGetValue(key, out var distribution)) {
                var stdDev = _parameter / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = _lap.Context.CreateContinuousDistribution(0, stdDev));
            }
            return distribution.Sample();
        }
    }
}
