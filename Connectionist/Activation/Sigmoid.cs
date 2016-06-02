using Icbld.BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Activation
{
    public class Sigmoid : IActivationFunction
    {
        public ActivationType Type { get { return ActivationType.Sigmoid; } }

        public float Calculate(float val)
        {
            return CpuMatrix._Sigmoid(val);
        }

        public IMatrix Calculate(IMatrix data)
        {
            return data.SigmoidActivation();
        }

        public IMatrix Derivative(IMatrix data)
        {
            return data.SigmoidDerivative();
        }
    }
}
