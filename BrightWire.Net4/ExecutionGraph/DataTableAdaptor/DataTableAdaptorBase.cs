using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    abstract class DataTableAdaptorBase : IDataSource
    {
        protected readonly ILinearAlgebraProvider _lap;
        protected readonly IDataTable _dataTable;

        public DataTableAdaptorBase(ILinearAlgebraProvider lap, IDataTable dataTable)
        {
            _lap = lap;
            _dataTable = dataTable;
        }

        public abstract bool IsSequential { get; }
        public abstract int InputSize { get; }
        public abstract int OutputSize { get; }
        public virtual int RowCount => _dataTable.RowCount;

        public abstract IMiniBatch Get(IReadOnlyList<int> rows);
        public abstract IDataSource GetFor(IDataTable dataTable);

        public virtual IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                Enumerable.Range(0, _dataTable.RowCount).ToList()
            };
        }

        public virtual void OnBatchProcessed(IContext context)
        {
            // nop
        }

        protected IMiniBatch _GetMiniBatch(IReadOnlyList<int> rows, IReadOnlyList<(float[], float[])> data)
        {
            var input = _lap.CreateMatrix(data.Count, InputSize, (x, y) => data[x].Item1[y]);
            var output = OutputSize > 0 ? _lap.CreateMatrix(data.Count, OutputSize, (x, y) => data[x].Item2[y]) : null;
            return new MiniBatch(rows, this, input, output);
        }

        protected IMiniBatch _GetSequentialMiniBatch(IReadOnlyList<int> rows, IReadOnlyList<(FloatMatrix Input, FloatMatrix Output)> data)
        {
            List<FloatVector> temp;
            var inputData = new Dictionary<int, List<FloatVector>>();
            var outputData = new Dictionary<int, List<FloatVector>>();
            foreach (var item in data) {
                var input = item.Input;
                var output = item.Output;
                for (int i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out temp))
                        inputData.Add(i, temp = new List<FloatVector>());
                    temp.Add(input.Row[i]);

                    if (output != null) {
                        if (!outputData.TryGetValue(i, out temp))
                            outputData.Add(i, temp = new List<FloatVector>());
                        temp.Add(output.Row[i]);
                    }
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = _lap.CreateMatrix(item.Value);
                IMatrix output = null;
                if (outputData.TryGetValue(item.Key, out temp))
                    output = _lap.CreateMatrix(temp);
                var type = (item.Key == 0)
                    ? MiniBatchType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchType.SequenceEnd
                        : MiniBatchType.Standard
                ;
                miniBatch.Add(type, input, output);
            }
            return miniBatch;
        }
    }
}
