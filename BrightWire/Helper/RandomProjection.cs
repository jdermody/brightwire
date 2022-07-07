using System;
using BrightData;
using BrightData.LinearAlegbra2;

namespace BrightWire.Helper
{
    /// <summary>
    /// Implements random projection
    /// </summary>
    internal class RandomProjection : IRandomProjection
    {
        public RandomProjection(LinearAlgebraProvider lap, uint fixedSize, uint reducedSize, int s = 3)
        {
            LinearAlgebraProvider = lap;
            Size = reducedSize;

            var c1 = MathF.Sqrt(3);
            var distribution = lap.Context.CreateCategoricalDistribution(new[] { 1.0f / (2f * s), 1f - (1.0f / s), 1.0f / (2f * s) });
            Matrix = LinearAlgebraProvider.CreateMatrix(fixedSize, reducedSize, (_, _) => (distribution.Sample() - 1f) * c1);
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

        public LinearAlgebraProvider LinearAlgebraProvider { get; }
	    public uint Size { get; }
		public IMatrix Matrix { get; }

	    public IVector Compute(IVector vector)
        {
            using var m = vector.Reshape(1, null);
            using var m2 = m.Multiply(Matrix);
            return LinearAlgebraProvider.CreateVector(m2.GetRow(0));
        }

        public IMatrix Compute(IMatrix matrix)
        {
            return matrix.Multiply(Matrix);
        }
    }
}
