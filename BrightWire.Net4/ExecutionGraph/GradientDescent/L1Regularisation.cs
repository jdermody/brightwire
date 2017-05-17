using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class L1Regularisation : Simple
    {
        float _lambda;

        public L1Regularisation(float lambda)
        {
            _lambda = lambda;
        }

        override public void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            var l1 = context.LearningRate * _lambda / context.RowCount;
            source.L1Regularisation(l1);
            base.Update(source, delta, context);
        }

        public void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _lambda = reader.ReadSingle();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(_lambda);
        }
    }
}
