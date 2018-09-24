using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Base class for data table based data adaptors
    /// </summary>
    /// <typeparam name="T">The type of the cached data</typeparam>
    abstract class DataTableAdaptorBase<T> : IDataSource
    {
        protected readonly int[] _dataColumnIndex;
        protected readonly int _dataTargetIndex;
        protected readonly ILinearAlgebraProvider _lap;
        protected readonly List<T> _data = new List<T>();

	    protected DataTableAdaptorBase(ILinearAlgebraProvider lap, IDataTable dataTable)
        {
            _lap = lap;
            _dataTargetIndex = dataTable.TargetColumnIndex;
            _dataColumnIndex = Enumerable.Range(0, dataTable.ColumnCount).Where(ci => ci != _dataTargetIndex).ToArray();
        }

        public int InputCount => 1;
        public abstract bool IsSequential { get; }
        public abstract int InputSize { get; }
        public abstract int OutputSize { get; }
        public virtual int RowCount => _data.Count;

        public abstract IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows);
        public abstract IDataSource CloneWith(IDataTable dataTable);

        public virtual IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                Enumerable.Range(0, _data.Count).ToList()
            };
        }

        public virtual void OnBatchProcessed(IContext context)
        {
            // nop
        }

        protected IReadOnlyList<T> _GetRows(IReadOnlyList<int> rows)
        {
            return rows.Select(i => _data[i]).ToList();
        }

        protected IMiniBatch _GetMiniBatch(IReadOnlyList<int> rows, IReadOnlyList<(float[][], float[])> data)
        {
            var inputList = new List<IGraphData>();
            for (int i = 0, len = data.First().Item1.Length; i < len; i++)
            {
	            var i1 = i;
		        inputList.Add(new MatrixGraphData(_lap.CreateMatrix(data.Count, InputSize, (x, y) => data[x].Item1[i1][y])));
	        }

	        var output = OutputSize > 0 ? _lap.CreateMatrix(data.Count, OutputSize, (x, y) => data[x].Item2[y]) : null;
            return new MiniBatch(rows, this, inputList, new MatrixGraphData(output));
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
                var input = _lap.CreateMatrixFromRows(item.Value);
                IMatrix output = null;
                if (outputData.TryGetValue(item.Key, out temp))
                    output = _lap.CreateMatrixFromRows(temp);
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                var inputList = new List<IGraphData> {
                    new MatrixGraphData(input)
                };
                miniBatch.Add(type, inputList, new MatrixGraphData(output));
            }
            return miniBatch;
        }
    }
}
