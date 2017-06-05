using System.IO;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// L2 regularisation
    /// </summary>
    class L2Regularisation : StochasticGradientDescent
    {
        float _lambda;

        public L2Regularisation(float lambda)
        {
            _lambda = lambda;
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context, bool hasAveragedBatchSize)
        {
            var l2 = 1.0f - (context.LearningRate * _lambda / context.BatchSize);
            float coefficient = 1f;
            //if (!hasAveragedBatchSize)
            //    coefficient /= context.BatchSize;
            _Update(source, delta, context, l2, coefficient);
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
