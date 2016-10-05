using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Activation
{
    internal class Relu : IActivationFunction
    {
        public ActivationType Type { get { return ActivationType.Relu; } }

        public float Calculate(float val)
        {
            return CpuMatrix._Relu(val);
        }

        public IMatrix Calculate(IMatrix data)
        {
            return data.ReluActivation();
        }

        public IMatrix Derivative(IMatrix layerOutput, IMatrix errorSignal)
        {
            return layerOutput.ReluDerivative();
        }
    }
}
