using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BrightWire.ExecutionGraph.Helper;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Base class for data table based data adapters
    /// </summary>
    /// <typeparam name="T">The type of the cached data</typeparam>
    public abstract class DataTableAdapterBase<T> : IDataSource
    {
        /// <summary>
        /// The data table column indices with features
        /// </summary>
        protected readonly uint[] _featureColumnIndices;

		/// <summary>
		/// Target column index
		/// </summary>
        protected readonly uint _targetColumnIndex;

		/// <summary>
		/// Data table
		/// </summary>
        protected readonly IRowOrientedDataTable _dataTable;

        /// <summary>
	    /// Constructor
	    /// </summary>
        /// <param name="dataTable"></param>
	    /// <param name="featureColumns"></param>
	    protected DataTableAdapterBase(IRowOrientedDataTable dataTable, uint[] featureColumns)
        {
            _dataTable = dataTable;
            _targetColumnIndex = dataTable.GetTargetColumnOrThrow();
            _featureColumnIndices = featureColumns;
            RowCount = dataTable.RowCount;
        }

	    /// <inheritdoc />
        public abstract uint InputSize { get; }
	    /// <inheritdoc />
        public abstract uint? OutputSize { get; }
	    /// <inheritdoc />
        public virtual uint RowCount { get; }

        /// <inheritdoc />
        public abstract IMiniBatch Get(uint[] rows);

        /// <inheritdoc />
        public abstract IDataSource CloneWith(IRowOrientedDataTable dataTable);

        /// <inheritdoc />
        public IDataTableVectoriser? InputVectoriser { get; protected set; } = null;

        /// <inheritdoc />
        public IDataTableVectoriser? OutputVectoriser { get; protected set; } = null;

        /// <inheritdoc />
        public virtual uint[][] GetSequentialBatches()
        {
            return new[] {
                RowCount.AsRange().ToArray()
            };
        }

        /// <summary>
        /// Returns data for the specified row indices
        /// </summary>
        /// <param name="rows">List of row indices</param>
        protected abstract IEnumerable<T> GetRows(uint[] rows);

        /// <summary>
		/// Creates a mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">List of input/output tuples</param>
        protected IMiniBatch GetMiniBatch(uint[] rows, (float[][] Input, float[] Output)[] data)
        {
            var numInputs = (uint)data[0].Input.Length;
            var inputList = new IGraphData[numInputs];
            var lap = _dataTable.Context.LinearAlgebraProvider2;

            for (uint i = 0; i < numInputs; i++) {
                var i1 = i;
                inputList[i] = lap.CreateMatrix((uint) data.Length, InputSize, (x, y) => data[(int) x].Input[i1][y]).AsGraphData();
            }

	        var output = OutputSize > 0 
                ? lap.CreateMatrix((uint)data.Length, (uint)OutputSize, (x, y) => data[(int)x].Output[y]).AsGraphData()
                : null;

            // TODO: change from single
            return new MiniBatch(rows, this, inputList.Single(), output);
        }

		/// <summary>
		/// Creates a sequential mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">List of input/output matrix tuples</param>
        protected IMiniBatch GetSequentialMiniBatch(uint[] rows, (IMatrix Input, IMatrix? Output)[] data)
        {
            List<ITensorSegment2>? temp;
            var inputData = new Dictionary<uint, List<ITensorSegment2>>();
            var outputData = new Dictionary<uint, List<ITensorSegment2>>();
            var lap = _dataTable.Context.LinearAlgebraProvider2;

            foreach (var (input, output) in data) {
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out temp))
                        inputData.Add(i, temp = new());
                    temp.Add(input.Row(i));

                    if (output != null) {
                        if (!outputData.TryGetValue(i, out temp))
                            outputData.Add(i, temp = new());
                        temp.Add(output.Row(i));
                    }
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                IGraphData? output = null;
                if (outputData.TryGetValue(item.Key, out temp))
                    output = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(temp)).AsGraphData();
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, input.AsGraphData(), output);
            }
            return miniBatch;
        }
    }
}
