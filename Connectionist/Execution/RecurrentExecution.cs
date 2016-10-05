using BrightWire.Helper;
using BrightWire.Models.ExecutionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Execution
{
    internal class RecurrentExecution : IRecurrentExecution
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<IRecurrentLayerExecution> _layer;
        readonly IVector _initialMemory;

        public RecurrentExecution(ILinearAlgebraProvider lap, IReadOnlyList<IRecurrentLayerExecution> layer, IVector initialMemory)
        {
            _lap = lap;
            _layer = layer;
            _initialMemory = initialMemory;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var item in _layer)
                    item.Dispose();
                _initialMemory.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public float[] InitialMemory { get { return _initialMemory.AsIndexable().ToArray(); } }

        public IReadOnlyList<IRecurrentOutput> Execute(IReadOnlyList<IVector> inputData)
        {
            var context = new List<IDisposableMatrixExecutionLine>();
            context.Add(new DisposableMatrixExecutionLine());
            context.Add(new DisposableMatrixExecutionLine(_initialMemory.ToRowMatrix()));

            var ret = new List<RecurrentOutput>();
            foreach (var item in inputData) {
                context[0].Assign(item.ToRowMatrix());
                        
                foreach (var action in _layer)
                    action.Activate(context);
                var memoryOutput = context[1].Current.AsIndexable().Rows.First();

                var output = context[0].Current.Row(0).AsIndexable();
                ret.Add(new RecurrentOutput(output, memoryOutput));
            }
            return ret;
        }

        public IReadOnlyList<IRecurrentOutput> Execute(IReadOnlyList<float[]> inputData)
        {
            var context = new List<IDisposableMatrixExecutionLine>();
            context.Add(new DisposableMatrixExecutionLine());
            context.Add(new DisposableMatrixExecutionLine());

            var ret = new List<RecurrentOutput>();
            using (var m2 = _initialMemory.ToRowMatrix()) {
                context[1].Assign(m2);
                foreach (var item in inputData) {
                    using (var curr = _lap.Create(item))
                    using (var curr2 = curr.ToRowMatrix()) {
                        context[0].Assign(curr2);

                        foreach (var action in _layer)
                            action.Activate(context);
                        var memoryOutput = context[1].Current.AsIndexable().Rows.First();

                        var output = context[0].Current.Row(0).AsIndexable();
                        ret.Add(new RecurrentOutput(output, memoryOutput));
                    }
                }
            }
            return ret;
        }

        public IRecurrentOutput ExecuteSingle(float[] data, float[] memory)
        {
            var context = new List<IDisposableMatrixExecutionLine>();
            context.Add(new DisposableMatrixExecutionLine());
            context.Add(new DisposableMatrixExecutionLine());

            //var ret = new List<Tuple<IIndexableVector, IIndexableVector>>();
            using (var m = _lap.Create(memory))
            using (var m2 = m.ToRowMatrix()) {
                context[1].Assign(m2);
                using (var curr = _lap.Create(data))
                using (var curr2 = curr.ToRowMatrix()) {
                    context[0].Assign(curr2);

                    foreach (var action in _layer)
                        action.Activate(context);
                    var memoryOutput = context[1].Current.AsIndexable().Rows.First();

                    var output = context[0].Current.Row(0).AsIndexable();
                    return new RecurrentOutput(output, memoryOutput);
                }
            }
        }
    }
}
