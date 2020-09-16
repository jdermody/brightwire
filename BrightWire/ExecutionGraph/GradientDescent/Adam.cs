using System;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Adam gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#Adam
    /// </summary>
    class Adam : RmsProp
    {
        float _decayRate2;
        IFloatMatrix _cache2;

        public Adam(float decay, float decay2, IFloatMatrix cache, IFloatMatrix cache2, IGradientDescentOptimisation updater) : base(decay, cache, updater)
        {
            _decayRate2 = decay2;
            _cache2 = cache2;
        }

        public override void Dispose()
        {
            _cache2.Dispose();
            base.Dispose();
        }

        public override void Update(IFloatMatrix source, IFloatMatrix delta, ILearningContext context)
        {
            var t = context.CurrentEpoch;

            using var deltaSquared = delta.PointwiseMultiply(delta);
            _cache.AddInPlace(delta, _decayRate, 1 - _decayRate);
            _cache2.AddInPlace(deltaSquared, _decayRate2, 1 - _decayRate2);

            using var mb = _cache.Clone();
            using var vb = _cache2.Clone();
            mb.Multiply(1f / (1f - Convert.ToSingle(Math.Pow(_decayRate, t))));
            vb.Multiply(1f / (1f - Convert.ToSingle(Math.Pow(_decayRate2, t))));
            using var vbSqrt = vb.Sqrt(1e-8f);
            using var delta2 = mb.PointwiseDivide(vbSqrt);
            _updater.Update(source, delta2, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);

            _decayRate2 = (uint)reader.ReadSingle();
            var rows = (uint)reader.ReadInt32();
            var columns = (uint)reader.ReadInt32();
            _cache2 = factory.LinearAlgebraProvider.CreateZeroMatrix(rows, columns);
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
