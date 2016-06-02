using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Icbld.BrightWire.Helper;

namespace Icbld.BrightWire.Connectionist.Execution.Layer
{
    public class RecurrentFeedForward : IRecurrentLayerExecution
    {
        readonly StandardFeedForward _layer;

        public RecurrentFeedForward(StandardFeedForward layer)
        {
            _layer = layer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
                _layer.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Activate(List<IDisposableMatrixExecutionLine> curr)
        {
            _layer.Activate(curr[0]);
        }
    }
}
