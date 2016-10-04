using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.DimensionalityReduction
{
    public class RandomProjection : IDisposable
    {
        readonly IMatrix _matrix;
        readonly ILinearAlgebraProvider _lap;
        readonly int _fixedSize, _reducedSize;

        public RandomProjection(ILinearAlgebraProvider lap, int fixedSize, int reducedSize, int s = 3)
        {
            _lap = lap;
            _fixedSize = fixedSize;
            _reducedSize = reducedSize;

            var c1 = Math.Sqrt(3);
            var distribution = new Categorical(new[] { 1.0 / (2 * s), 1 - (1.0 / s), 1.0 / (2 * s) });
            _matrix = _lap.Create(fixedSize, reducedSize, (i, j) => Convert.ToSingle((distribution.Sample() - 1) * c1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _matrix.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }
        public int Size { get { return _reducedSize; } }
        public IMatrix Matrix { get { return _matrix; } }

        public IVector Compute(IVector vector)
        {
            using (var m = vector.ToRowMatrix())
            using (var m2 = m.Multiply(_matrix))
                return m2.Row(0);
        }

        public IMatrix Compute(IMatrix matrix)
        {
            return matrix.Multiply(_matrix);
        }

        public static int MinDim(int count, double eps = 0.1)
        {
            var denominator = (Math.Pow(eps, 2) / 2.0) - (Math.Pow(eps, 3) / 3.0);
            return Convert.ToInt32(Math.Round((4.0 * Math.Log(count) / denominator)));
        }
    }
}
