﻿using System;
using BrightData;
using BrightData.Distributions;

namespace BrightWire.Helper
{
    /// <summary>
    /// Implements random projection
    /// </summary>
    class RandomProjection : IRandomProjection
    {
        public RandomProjection(ILinearAlgebraProvider lap, uint fixedSize, uint reducedSize, int s = 3)
        {
            LinearAlgebraProvider = lap;
            Size = reducedSize;

            var c1 = MathF.Sqrt(3);
            var distribution = new CategoricalDistribution(lap.Context, new[] { 1.0f / (2f * s), 1f - (1.0f / s), 1.0f / (2f * s) });
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