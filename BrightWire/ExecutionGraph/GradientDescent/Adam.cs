using System;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Adam gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#Adam
    /// </summary>
    internal class Adam(float decay, float decay2, IMatrix<float> cache, IMatrix<float> cache2, IGradientDescentOptimisation updater)
        : RmsProp(decay, cache, updater)
    {
        float _decay2 = decay2;
        IMatrix<float> _cache2 = cache2;

        public override void Dispose()
        {
            _cache2.Dispose();
            base.Dispose();
        }

        public override void Update(IMatrix<float> source, IMatrix<float> delta, ILearningContext context)
        {
            var t = context.CurrentEpoch;

            using var deltaSquared = delta.PointwiseMultiply(delta);
            _cache.AddInPlace(delta, _decayRate, 1 - _decayRate);
            _cache2.AddInPlace(deltaSquared, _decay2, 1 - _decay2);

            using var mb = _cache.Multiply(1f / (1f - MathF.Pow(_decayRate, t)));
            using var vb = _cache2.Multiply(1f / (1f - MathF.Pow(_decay2, t)));
            using var vbSqrt = vb.Sqrt();
            using var delta2 = mb.PointwiseDivide(vbSqrt);
            _updater.Update(source, delta2, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);

            _decay2 = reader.ReadSingle();
            var rows = (uint)reader.ReadInt32();
            var columns = (uint)reader.ReadInt32();
            _cache2 = factory.LinearAlgebraProvider.CreateMatrix(rows, columns, true);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_decay2);
            writer.Write(_cache2.RowCount);
            writer.Write(_cache2.ColumnCount);
        }
    }
}
