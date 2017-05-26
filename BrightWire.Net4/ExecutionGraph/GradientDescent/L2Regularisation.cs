using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class L2Regularisation : Simple
    {
        float _lambda;

        public L2Regularisation(float lambda)
        {
            _lambda = lambda;
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            var l2 = 1.0f - (context.LearningRate * _lambda / context.RowCount);
            base._Update(source, delta, context, l2);
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
