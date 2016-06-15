using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Execution.Layer
{
    public class SimpleRecurrent : IRecurrentLayerExecution
    {
        readonly RecurrentLayerComponent _w;

        public SimpleRecurrent(RecurrentLayerComponent w)
        {
            _w = w;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
                _w.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Activate(List<IDisposableMatrixExecutionLine> curr)
        {
            var activation = _w.Activate(curr.Select(c => c.Current));
            curr[0].Assign(activation);
            curr[1].Replace(activation);
        }
    }
}
