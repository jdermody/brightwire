using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// L1 regularisation
    /// </summary>
    internal class L1Regularisation : StochasticGradientDescent
    {
        float _lambda;

        public L1Regularisation(float lambda)
        {
            _lambda = lambda;
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            var l1 = context.LearningRate * _lambda;
            source.L1Regularisation(l1);
            base.Update(source, delta, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _lambda = reader.ReadSingle();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_lambda);
        }
    }
}
