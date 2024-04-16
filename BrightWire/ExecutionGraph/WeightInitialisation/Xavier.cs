using System;
using System.Collections.Generic;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Xavier weight initialisation
    /// http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
    /// </summary>
    internal class Xavier(LinearAlgebraProvider<float> lap, float parameter = 6) : IWeightInitialisation
    {
        readonly float _parameter = MathF.Sqrt(parameter);
        readonly Dictionary<(uint, uint), IContinuousDistribution> _distributionTable = new();

        public IVector<float> CreateBias(uint size)
        {
            return lap.CreateVector(size, true);
        }

        public IMatrix<float> CreateWeight(uint rows, uint columns)
        {
            return lap.CreateMatrix(rows, columns, (_, _) => GetWeight(rows, columns));
        }

        public float GetWeight(uint inputSize, uint outputSize)
        {
            var key = (inputSize, outputSize);
            if (!_distributionTable.TryGetValue(key, out var distribution)) {
                var stdDev = _parameter / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = lap.Context.CreateContinuousDistribution(0, stdDev));
            }
            return distribution.Sample();
        }
    }
}
