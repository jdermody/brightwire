using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BrightWire.ExecutionGraph.Helper;
using BrightData;
using BrightData.DataTable2;
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
        protected readonly BrightDataTable _dataTable;

        /// <summary>
	    /// Constructor
	    /// </summary>
        /// <param name="dataTable"></param>
	    /// <param name="featureColumns"></param>
	    protected DataTableAdapterBase(BrightDataTable dataTable, uint[] featureColumns)
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
        public abstract IDataSource CloneWith(BrightDataTable dataTable);

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
        protected IMiniBatch GetSequentialMiniBatch(uint[] rows, (IMatrixInfo Input, IMatrixInfo? Output)[] data)
        {
            List<ITensorSegment2>? temp;
            var inputData = new List<List<IVectorInfo>>();
            var outputData = new List<List<IVectorInfo>>();
            var lap = _dataTable.Context.LinearAlgebraProvider2;

            foreach (var (input, output) in data) {
                var inputList = new List<IVectorInfo>();
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    inputList.Add(input.GetRow(i));
                    if (output != null) {
                        var outputList = new List<IVectorInfo>();
                        outputList.Add(output.GetRow(i));
                        outputData.Add(outputList);
                    }
                }
                inputData.Add(inputList);
            }

            var miniBatch = new MiniBatch(rows, this);
            var index = 0;
            foreach (var item in inputData) {
                var input = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item));
                IGraphData? output = null;
                if (outputData.Any())
                    output = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(outputData[index])).AsGraphData();
                var type = (index == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : index == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, input.AsGraphData(), output);
                ++index;
            }
            return miniBatch;
        }
    }
}
