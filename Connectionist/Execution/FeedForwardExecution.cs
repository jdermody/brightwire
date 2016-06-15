using BrightWire.Connectionist.Execution.Layer;
using BrightWire.Helper;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Execution
{
    public class FeedForwardExecution : IStandardExecution
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<StandardFeedForward> _layer;

        public FeedForwardExecution(ILinearAlgebraProvider lap, IReadOnlyList<StandardFeedForward> layer)
        {
            _lap = lap;
            _layer = layer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var item in _layer)
                    item.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IVector Execute(float[] inputData)
        {
            using (var curr = _lap.Create(inputData))
                return Execute(curr);
        }

        void _Execute(IDisposableMatrixExecutionLine m)
        {
            foreach (var layer in _layer) {
                layer.Activate(m);
            }
        }

        public IVector Execute(IVector inputData)
        {
            using (var m = new DisposableMatrixExecutionLine(inputData.ToRowMatrix())) {
                _Execute(m);
                return m.Current.Row(0);
            }
        }

        public IMatrix Execute(IMatrix inputData)
        {
            using (var m = new DisposableMatrixExecutionLine()) {
                _Execute(m);
                return m.Pop();
            }
        }
    }
}
