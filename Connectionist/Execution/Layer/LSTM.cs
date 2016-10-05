using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Execution.Layer
{
    internal class Lstm : IRecurrentLayerExecution
    {
        readonly RecurrentLayerComponent _c, _i, _f, _o;
        readonly IActivationFunction _activation;

        public Lstm(RecurrentLayerComponent c, RecurrentLayerComponent i, RecurrentLayerComponent f, RecurrentLayerComponent o, IActivationFunction activation)
        {
            _c = c;
            _i = i;
            _f = f;
            _o = o;
            _activation = activation;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _c.Dispose();
                _i.Dispose();
                _f.Dispose();
                _o.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Activate(List<IDisposableMatrixExecutionLine> curr)
        {
            var curr2 = curr.Select(c => c.Current).ToList();
            using (var a = _c.Activate(curr2))
            using (var i = _i.Activate(curr2))
            using (var f = _f.Activate(curr2))
            using (var o = _o.Activate(curr2))
            using (var f2 = f.PointwiseMultiply(curr[1].Current)) {
                var ct = a.PointwiseMultiply(i);
                ct.AddInPlace(f2);
                using (var cta = _activation.Calculate(ct)) {
                    curr[0].Assign(o.PointwiseMultiply(cta));
                    curr[1].Assign(ct);
                }
            }
        }
    }
}
