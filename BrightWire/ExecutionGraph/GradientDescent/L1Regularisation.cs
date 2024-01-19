using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// L1 regularisation
    /// </summary>
    internal class L1Regularisation(float lambda) : StochasticGradientDescent
    {
        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            var l1 = context.LearningRate * lambda;
            source.L1RegularisationInPlace(l1);
            base.Update(source, delta, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            lambda = reader.ReadSingle();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(lambda);
        }
    }
}
