using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Helper;
using BrightTable;
using System;
using BrightData;
using BrightData.FloatTensors;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Base class for data table based data adaptors
    /// </summary>
    /// <typeparam name="T">The type of the cached data</typeparam>
    public abstract class DataTableAdaptorBase<T> : IDataSource
    {
        /// <summary>
        /// The data table columns with attributes
        /// </summary>
        protected readonly uint[] _dataColumnIndex;

		/// <summary>
		/// Target column index
		/// </summary>
        protected readonly uint _dataTargetIndex;

		/// <summary>
		/// Linear algebra provider
		/// </summary>
        protected readonly ILinearAlgebraProvider _lap;

		/// <summary>
		/// The list of raw row data
		/// </summary>
        protected readonly List<T> _data = new List<T>();

	    protected DataTableAdaptorBase(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable)
        {
            _lap = lap;
            _dataTargetIndex = dataTable.GetTargetColumn() ?? throw new ArgumentException("");
            _dataColumnIndex = dataTable.ColumnCount.AsRange().Where(ci => ci != _dataTargetIndex).ToArray();
        }

	    /// <inheritdoc />
	    public uint InputCount => 1;
	    /// <inheritdoc />
        public abstract bool IsSequential { get; }
	    /// <inheritdoc />
        public abstract uint InputSize { get; }
	    /// <inheritdoc />
        public abstract uint? OutputSize { get; }
	    /// <inheritdoc />
        public virtual uint RowCount => (uint)_data.Count;

        public abstract IMiniBatch Get(IExecutionContext executionContext, uint[] rows);
        public abstract IDataSource CloneWith(IRowOrientedDataTable dataTable);

	    /// <inheritdoc />
        public virtual uint[][] GetBuckets()
        {
            return new[] {
                _data.Count.AsRange().ToArray()
            };
        }

	    /// <inheritdoc />
        public virtual void OnBatchProcessed(IContext context)
        {
            // nop
        }

		/// <summary>
		/// Returns the row data
		/// </summary>
		/// <param name="rows">List of row indices</param>
        protected IEnumerable<T> _GetRows(uint[] rows)
        {
            return rows.Select(i => _data[(int)i]);
        }

		/// <summary>
		/// Creates a mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">List of input/output tuples</param>
        protected IMiniBatch _GetMiniBatch(uint[] rows, (float[][] Input, float[] Output)[] data)
        {
            var numInputs = (uint)data[0].Input.Length;
            var inputList = new IGraphData[numInputs];
            for (uint i = 0; i < numInputs; i++) {
                var i1 = i;
                inputList[i] = new MatrixGraphData(_lap.CreateMatrix((uint)data.Length, InputSize, (x, y) => data[(int)x].Input[i1][y]));
            }

	        var output = OutputSize > 0 
                ? _lap.CreateMatrix((uint)data.Length, (uint)OutputSize, (x, y) => data[(int)x].Output[y]) 
                : null;

            return new MiniBatch(rows, this, inputList, new MatrixGraphData(output));
        }

		/// <summary>
		/// Creates a sequential mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">List of input/output matrix tuples</param>
        protected IMiniBatch _GetSequentialMiniBatch(uint[] rows, (Matrix<float> Input, Matrix<float> Output)[] data)
        {
            List<Vector<float>> temp;
            var inputData = new Dictionary<uint, List<Vector<float>>>();
            var outputData = new Dictionary<uint, List<Vector<float>>>();

            foreach (var item in data) {
                var input = item.Input;
                var output = item.Output;
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out temp))
                        inputData.Add(i, temp = new List<Vector<float>>());
                    temp.Add(input.Row(i));

                    if (output != null) {
                        if (!outputData.TryGetValue(i, out temp))
                            outputData.Add(i, temp = new List<Vector<float>>());
                        temp.Add(output.Row(i));
                    }
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = _lap.CreateMatrixFromRows(item.Value);
                IFloatMatrix output = null;
                if (outputData.TryGetValue(item.Key, out temp))
                    output = _lap.CreateMatrixFromRows(temp);
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                var inputList = new IGraphData[] {
                    new MatrixGraphData(input)
                };
                miniBatch.Add(type, inputList, new MatrixGraphData(output));
            }
            return miniBatch;
        }
    }
}
