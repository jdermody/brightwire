using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Execution.Layer
{
    internal class StandardFeedForward : IDisposable
    {
        readonly IMatrix _weight;
        readonly IVector _bias;
        readonly IActivationFunction _activation;

        public StandardFeedForward(IMatrix weight, IVector bias, IActivationFunction activation)
        {
            _weight = weight;
            _bias = bias;
            _activation = activation;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _weight.Dispose();
                _bias.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Activate(IDisposableMatrixExecutionLine m)
        {
            // multiply weights
            m.Assign(m.Current.Multiply(_weight));

            // add bias
            m.Current.AddToEachRow(_bias);

            // activate output
            m.Assign(_activation.Calculate(m.Current));
        }

        public IMatrix Execute(IMatrix input)
        {
            var ret = input.Multiply(_weight);
            ret.AddToEachRow(_bias);
            return ret;
        }

        public IMatrix Activate(IMatrix input)
        {
            using (var temp = input.Multiply(_weight)) {
                temp.AddToEachRow(_bias);
                return _activation.Calculate(temp);
            }
        }
    }
}
