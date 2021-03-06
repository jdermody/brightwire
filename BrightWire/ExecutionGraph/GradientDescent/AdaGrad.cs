﻿using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// AdaGrad gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#AdaGrad
    /// </summary>
    internal class AdaGrad : IGradientDescentOptimisation
    {
        protected IFloatMatrix _cache;
        protected IGradientDescentOptimisation _updater;

        public AdaGrad(IFloatMatrix cache, IGradientDescentOptimisation updater)
        {
            _cache = cache;
            _updater = updater;
        }

        public virtual void Dispose()
        {
            _cache.Dispose();
        }

        public virtual void Update(IFloatMatrix source, IFloatMatrix delta, ILearningContext context)
        {
            using var deltaSquared = delta.PointwiseMultiply(delta);
            _cache.AddInPlace(deltaSquared);

            using var cachedSqrt = _cache.Sqrt();
            using var delta2 = delta.PointwiseDivide(cachedSqrt);
            _updater.Update(source, delta2, context);
        }

        public virtual void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var rows = (uint)reader.ReadInt32();
            var columns = (uint)reader.ReadInt32();
            _cache = factory.LinearAlgebraProvider.CreateZeroMatrix(rows, columns);
            _updater = factory.CreateGradientDescentOptimisation(reader);
        }

        public virtual void WriteTo(BinaryWriter writer)
        {
            writer.Write(_cache.RowCount);
            writer.Write(_cache.ColumnCount);
            writer.Write(_updater);
        }
    }
}
