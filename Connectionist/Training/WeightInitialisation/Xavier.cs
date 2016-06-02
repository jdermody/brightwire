using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Training.WeightInitialisation
{
    public class Xavier : IWeightInitialisation
    {
        readonly Dictionary<Tuple<int, int>, IContinuousDistribution> _distributionTable = new Dictionary<Tuple<int, int>, IContinuousDistribution>();
        readonly Random _random;

        public Xavier(bool stochastic)
        {
            _random = stochastic ? new Random() : new Random(0);
        }

        public float GetBias()
        {
            return 0f;
        }

        public float GetWeight(int inputSize, int outputSize, int i, int j)
        {
            var key = Tuple.Create(inputSize, outputSize);
            IContinuousDistribution distribution;
            if (!_distributionTable.TryGetValue(key, out distribution)) {
                var stdDev = Math.Sqrt(6) / (inputSize + outputSize);
                _distributionTable.Add(key, distribution = new Normal(0, stdDev, _random));
            }
            return Convert.ToSingle(distribution.Sample());
        }
    }
}
