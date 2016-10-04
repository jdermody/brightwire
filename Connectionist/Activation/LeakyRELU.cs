using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Activation
{
    public class LeakyRelu : IActivationFunction
    {
        public ActivationType Type { get { return ActivationType.LeakyRelu; } }

        public float Calculate(float val)
        {
            return CpuMatrix._LeakyRelu(val);
        }

        public IMatrix Calculate(IMatrix data)
        {
            return data.LeakyReluActivation();
        }

        public IMatrix Derivative(IMatrix layerOutput, IMatrix errorSignal)
        {
            return layerOutput.LeakyReluDerivative();
        }
    }
}
