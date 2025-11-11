using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// L2 regularisation
    /// </summary>
    internal class L2Regularisation(float lambda) : StochasticGradientDescent
    {
        float _lambda = lambda;

        public override void Update(IMatrix<float> source, IMatrix<float> delta, ILearningContext context)
        {
            var l2 = 1.0f - (context.LearningRate * _lambda);
            StochasticGradientDescent.Update(source, delta, context, l2, context.LearningRate);
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
