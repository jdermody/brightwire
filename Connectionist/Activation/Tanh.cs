using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Activation
{
    internal class Tanh : IActivationFunction
    {
        public ActivationType Type { get { return ActivationType.Tanh; } }

        public float Calculate(float val)
        {
            return CpuMatrix._Tanh(val);
        }

        public IMatrix Calculate(IMatrix data)
        {
            return data.TanhActivation();
        }

        public IMatrix Derivative(IMatrix layerOutput, IMatrix errorSignal)
        {
            return layerOutput.TanhDerivative();
        }
    }
}
