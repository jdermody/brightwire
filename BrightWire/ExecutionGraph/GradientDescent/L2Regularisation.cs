﻿using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// L2 regularisation
    /// </summary>
    internal class L2Regularisation : StochasticGradientDescent
    {
        float _lambda;

        public L2Regularisation(float lambda)
        {
            _lambda = lambda;
        }

        public override void Update(IFloatMatrix source, IFloatMatrix delta, ILearningContext context)
        {
            var l2 = 1.0f - (context.BatchLearningRate * _lambda);
            Update(source, delta, context, l2, context.BatchLearningRate);
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
