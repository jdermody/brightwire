using MathNet.Numerics.Distributions;
using System;

namespace BrightWire.Helper
{
    /// <summary>
    /// Implements random projection
    /// </summary>
    class RandomProjection : IRandomProjection
    {
	    readonly uint _fixedSize;

		public RandomProjection(ILinearAlgebraProvider lap, uint fixedSize, uint reducedSize, int s = 3)
        {
            LinearAlgebraProvider = lap;
            _fixedSize = fixedSize;
            Size = reducedSize;

            var c1 = Math.Sqrt(3);
            var distribution = new Categorical(new[] { 1.0 / (2 * s), 1 - (1.0 / s), 1.0 / (2 * s) });
            Matrix = LinearAlgebraProvider.CreateMatrix(fixedSize, reducedSize, (i, j) => Convert.ToSingle((distribution.Sample() - 1) * c1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Matrix.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get; }
	    public uint Size { get; }
		public IMatrix Matrix { get; }

	    public IVector Compute(IVector vector)
        {
            using (var m = vector.ReshapeAsRowMatrix())
            using (var m2 = m.Multiply(Matrix))
                return m2.Row(0);
        }

        public IMatrix Compute(IMatrix matrix)
        {
            return matrix.Multiply(Matrix);
        }
    }
}
