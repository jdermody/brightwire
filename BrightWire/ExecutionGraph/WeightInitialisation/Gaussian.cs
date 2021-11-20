using System;
using BrightData;

namespace BrightWire.ExecutionGraph.WeightInitialisation
{
    /// <summary>
    /// Initialises weights randomly based on a gaussian distribution
    /// </summary>
    internal class Gaussian : IWeightInitialisation
    {
        readonly IContinuousDistribution _distribution;
        readonly GaussianVarianceCalibration _varianceCalibration;
        readonly GaussianVarianceCount _varianceCount;
        readonly ILinearAlgebraProvider _lap;
        readonly bool _zeroBias;

        public Gaussian(
            ILinearAlgebraProvider lap, 
            bool zeroInitialBias = true, 
            float stdDev = 0.1f, 
            GaussianVarianceCalibration varianceCalibration = GaussianVarianceCalibration.SquareRootN,
            GaussianVarianceCount varianceCount = GaussianVarianceCount.FanIn
        ) {
            _lap = lap;
            _zeroBias = zeroInitialBias;
            _varianceCalibration = varianceCalibration;
            _varianceCount = varianceCount;
            _distribution = _lap.Context.CreateNormalDistribution(0, stdDev);
        }

        float GetBias()
        {
            return _zeroBias ? 0f : _distribution.Sample();
        }

        float GetWeight(uint inputSize, uint outputSize)
        {
            uint n = 0;
            if (_varianceCount == GaussianVarianceCount.FanIn || _varianceCount == GaussianVarianceCount.FanInFanOut)
                n += inputSize;
            if (_varianceCount == GaussianVarianceCount.FanOut || _varianceCount == GaussianVarianceCount.FanInFanOut)
                n += outputSize;

            var sample = _distribution.Sample();
            if(n > 0) {
                if (_varianceCalibration == GaussianVarianceCalibration.SquareRootN)
                    sample *= MathF.Sqrt(n);
                else if (_varianceCalibration == GaussianVarianceCalibration.SquareRoot2N)
                    sample *= MathF.Sqrt(2.0f / n);
            }

            return sample;
        }

        public IFloatVector CreateBias(uint size)
        {
            return _lap.CreateVector(size, _ => GetBias());
        }

        public IFloatMatrix CreateWeight(uint rows, uint columns)
        {
            return _lap.CreateMatrix(rows, columns, (_, _) => GetWeight(rows, columns));
        }
    }
}
