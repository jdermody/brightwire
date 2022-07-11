using System;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Adam gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#Adam
    /// </summary>
    internal class Adam : RmsProp
    {
        float _decayRate2;
        IMatrix _cache2;

        public Adam(float decay, float decay2, IMatrix cache, IMatrix cache2, IGradientDescentOptimisation updater) : base(decay, cache, updater)
        {
            _decayRate2 = decay2;
            _cache2 = cache2;
        }

        public override void Dispose()
        {
            _cache2.Dispose();
            base.Dispose();
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            var t = context.CurrentEpoch;

            using var deltaSquared = delta.PointwiseMultiply(delta);
            _cache.AddInPlace(delta, _decayRate, 1 - _decayRate);
            _cache2.AddInPlace(deltaSquared, _decayRate2, 1 - _decayRate2);

            using var mb = _cache.Multiply(1f / (1f - MathF.Pow(_decayRate, t)));
            using var vb = _cache2.Multiply(1f / (1f - MathF.Pow(_decayRate2, t)));
            using var vbSqrt = vb.Sqrt();
            using var delta2 = mb.PointwiseDivide(vbSqrt);
            _updater.Update(source, delta2, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);

            _decayRate2 = (uint)reader.ReadSingle();
            var rows = (uint)reader.ReadInt32();
            var columns = (uint)reader.ReadInt32();
            _cache2 = factory.LinearAlgebraProvider.CreateMatrix(rows, columns);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_decayRate2);
            writer.Write(_cache2.RowCount);
            writer.Write(_cache2.ColumnCount);
        }
    }
}
