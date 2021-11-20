using System;
using BrightData;

namespace BrightWire.Helper
{
    /// <summary>
    /// Implements random projection
    /// </summary>
    internal class RandomProjection : IRandomProjection
    {
        public RandomProjection(ILinearAlgebraProvider lap, uint fixedSize, uint reducedSize, int s = 3)
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

        public ILinearAlgebraProvider LinearAlgebraProvider { get; }
	    public uint Size { get; }
		public IFloatMatrix Matrix { get; }

	    public IFloatVector Compute(IFloatVector vector)
        {
            using var m = vector.ReshapeAsRowMatrix();
            using var m2 = m.Multiply(Matrix);
            return m2.Row(0);
        }

        public IFloatMatrix Compute(IFloatMatrix matrix)
        {
            return matrix.Multiply(Matrix);
        }
    }
}
