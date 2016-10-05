using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Training.WeightInitialisation
{
    internal class Gaussian : IWeightInitialisation
    {
        readonly IContinuousDistribution _distribution;

        public Gaussian(bool stochastic, double stdDev = 0.1)
        {
            _distribution = stochastic ? new Normal(0, stdDev) : new Normal(0, stdDev, new Random(0));
            var test = _distribution.Sample();
        }

        public float GetBias()
        {
            return Convert.ToSingle(_distribution.Sample());
        }

        public float GetWeight(int inputSize, int outputSize, int i, int j)
        {
            var ret = Convert.ToSingle(_distribution.Sample() / Math.Sqrt(inputSize));
            return ret;
        }
    }
}
