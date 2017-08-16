using MathNet.Numerics.Distributions;
using System;

namespace BrightWire.Helper
{
    /// <summary>
    /// Implements random projection
    /// </summary>
    internal class RandomProjection : IRandomProjection
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
            _matrix = _lap.CreateMatrix(fixedSize, reducedSize, (i, j) => Convert.ToSingle((distribution.Sample() - 1) * c1));
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
    }
}
