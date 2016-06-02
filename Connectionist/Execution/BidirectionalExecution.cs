using Icbld.BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Execution
{
    public class BidirectionalExecution : IBidirectionalRecurrentExecution
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<Tuple<IRecurrentLayerExecution, IRecurrentLayerExecution>> _layer;
        readonly IMatrix _forwardMemory, _backwardMemory, _combined;
        readonly int _padding;

        public BidirectionalExecution(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<IRecurrentLayerExecution, IRecurrentLayerExecution>> layer, IVector forwardMemory, IVector backwardMemory, int padding)
        {
            _lap = lap;
            _layer = layer;
            _padding = padding;
            _forwardMemory = forwardMemory.ToRowMatrix();
            _backwardMemory = backwardMemory.ToRowMatrix();
            _combined = _forwardMemory.ConcatRows(_backwardMemory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var item in _layer) {
                    item.Item1.Dispose();
                    item.Item2.Dispose();
                }
                _forwardMemory.Dispose();
                _backwardMemory.Dispose();
                _combined.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IReadOnlyList<Tuple<IIndexableVector, IIndexableVector>> Execute(IReadOnlyList<IVector> inputData)
        {
            var forwardContext = new List<IDisposableMatrixExecutionLine>();
            forwardContext.Add(new DisposableMatrixExecutionLine());
            forwardContext.Add(new DisposableMatrixExecutionLine(_forwardMemory));

            var backwardContext = new List<IDisposableMatrixExecutionLine>();
            backwardContext.Add(new DisposableMatrixExecutionLine());
            backwardContext.Add(new DisposableMatrixExecutionLine(_backwardMemory));

            var len = inputData.Count;
            var data = inputData.Select(d => d.ToRowMatrix()).ToList();
            var forwardOutput = new Dictionary<int, Tuple<IMatrix, IMatrix>>();
            var backwardOutput = new Dictionary<int, Tuple<IMatrix, IMatrix>>();
            for (var i = 0; i < len; i++) {
                var bi = len - i - 1;
                forwardContext[0].Assign(data[i]);
                backwardContext[0].Assign(data[bi]);
                foreach (var layer in _layer.Where(l => l.Item1 != null && l.Item2 != null)) {
                    layer.Item1.Activate(forwardContext);
                    layer.Item2.Activate(backwardContext);
                    forwardOutput[i] = Tuple.Create(forwardContext[0].Current, forwardContext[1].Current);
                    backwardOutput[bi] = Tuple.Create(backwardContext[0].Current, backwardContext[1].Current);
                }
            }

            // merge the forward and backward outputs
            var singleInput = new List<Tuple<IMatrix, IMatrix>>();
            for (var i = 0; i < len; i++) {
                if (i + _padding < len) {
                    var input = forwardOutput[i].Item1.ConcatRows(backwardOutput[i + _padding].Item1);
                    var memory = forwardOutput[i].Item2.ConcatRows(backwardOutput[i + _padding].Item2);
                    singleInput.Add(Tuple.Create(input, memory));
                }else {
                    var forward = forwardOutput[i].Item1;
                    var lastMemory = forwardOutput[i].Item2;
                    var input = forward.ConcatRows(_lap.Create(forward.RowCount, forward.ColumnCount, 0f));
                    var memory = lastMemory.ConcatRows(_lap.Create(lastMemory.RowCount, lastMemory.ColumnCount, 0f));
                    singleInput.Add(Tuple.Create(input, memory));
                }
            }

            // activate the unidirectional layers
            var singleOutput = new Dictionary<int, Tuple<IMatrix, IMatrix>>();
            for (var i = 0; i < len; i++) {
                forwardContext[0].Assign(singleInput[i].Item1);
                forwardContext[1].Assign(singleInput[i].Item2);
                foreach (var layer in _layer.Where(l => l.Item1 != null && l.Item2 == null)) {
                    layer.Item1.Activate(forwardContext);
                    singleOutput[i] = Tuple.Create(forwardContext[0].Current, forwardContext[1].Current);
                }
            }

            // return the output
            var ret = new List<Tuple<IIndexableVector, IIndexableVector>>();
            foreach (var item in singleOutput.OrderBy(k => k.Key)) {
                using (var ir = item.Value.Item1.Row(0))
                using (var mr = item.Value.Item2.Row(0))
                    ret.Add(Tuple.Create(ir.AsIndexable(), mr.AsIndexable()));
            }
            return ret;
        }

        public IReadOnlyList<Tuple<IIndexableVector, IIndexableVector>> Execute(IReadOnlyList<float[]> inputData)
        {
            var list = inputData.Select(r => _lap.Create(r)).ToList();
            var ret = Execute(list);
            foreach (var item in list)
                item.Dispose();
            return ret;
        }
    }
}
