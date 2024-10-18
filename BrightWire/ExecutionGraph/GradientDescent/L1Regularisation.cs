using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// L1 regularisation
    /// </summary>
    internal class L1Regularisation(float lambda) : StochasticGradientDescent
    {
        float _lambda = lambda;

        public override void Update(IMatrix<float> source, IMatrix<float> delta, ILearningContext context)
        {
            var l1 = context.LearningRate * _lambda;
            source.L1RegularisationInPlace(l1);
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
