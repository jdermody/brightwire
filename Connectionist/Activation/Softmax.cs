using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.Connectionist.Activation
{
    public class Softmax : IActivationFunction
    {
        public ActivationType Type
        {
            get
            {
                return ActivationType.Softmax;
            }
        }

        public float Calculate(float val)
        {
            // cannot calculate a single value of softmax
            throw new NotImplementedException();
        }

        public IMatrix Calculate(IMatrix data)
        {
            return data.SoftmaxActivation();
        }

        public IMatrix Derivative(IMatrix data)
        {
            return data.SoftmaxDerivative();
        }
    }
}
