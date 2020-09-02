using System.IO;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// AdaGrad gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#AdaGrad
    /// </summary>
    class AdaGrad : IGradientDescentOptimisation
    {
        protected IMatrix _cache;
        protected IGradientDescentOptimisation _updater;

        public AdaGrad(IMatrix cache, IGradientDescentOptimisation updater)
        {
            _cache = cache;
            _updater = updater;
        }

        public virtual void Dispose()
        {
            _cache.Dispose();
        }

        public virtual void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            using (var deltaSquared = delta.PointwiseMultiply(delta)) {
                _cache.AddInPlace(deltaSquared);

                using (var cachedSqrt = _cache.Sqrt(1e-8f))
                using (var delta2 = delta.PointwiseDivide(cachedSqrt)) {
                    _updater.Update(source, delta2, context);
                }
            }
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
