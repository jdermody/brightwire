using System.Collections.Generic;
using System.Linq;
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
		/// Linear algebra provider
		/// </summary>
        protected readonly ILinearAlgebraProvider _lap;

		/// <summary>
		/// The list of raw row data
		/// </summary>
        protected readonly List<T> _data = new List<T>();

	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="lap"></param>
	    /// <param name="dataTable"></param>
	    /// <param name="featureColumns"></param>
	    protected DataTableAdapterBase(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint[] featureColumns)
        {
            _lap = lap;
            _targetColumnIndex = dataTable.GetTargetColumnOrThrow();
            _featureColumnIndices = featureColumns;
        }

	    /// <inheritdoc />
        public abstract uint InputSize { get; }
	    /// <inheritdoc />
        public abstract uint? OutputSize { get; }
	    /// <inheritdoc />
        public virtual uint RowCount => (uint)_data.Count;

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
                _data.Count.AsRange().ToArray()
            };
        }

        /// <summary>
		/// Returns the row data
		/// </summary>
		/// <param name="rows">List of row indices</param>
        protected IEnumerable<T> GetRows(uint[] rows)
        {
            return rows.Select(i => _data[(int)i]);
        }

		/// <summary>
		/// Creates a mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">List of input/output tuples</param>
        protected IMiniBatch GetMiniBatch(uint[] rows, (float[][] Input, float[] Output)[] data)
        {
            var numInputs = (uint)data[0].Input.Length;
            var inputList = new IGraphData[numInputs];
            for (uint i = 0; i < numInputs; i++) {
                var i1 = i;
                inputList[i] = new MatrixGraphData(_lap.CreateMatrix((uint)data.Length, InputSize, (x, y) => data[(int)x].Input[i1][y]));
            }

	        var output = OutputSize > 0 
                ? new MatrixGraphData(_lap.CreateMatrix((uint)data.Length, (uint)OutputSize, (x, y) => data[(int)x].Output[y]))
                : null;

            // TODO: change from single
            return new MiniBatch(rows, this, inputList.Single(), output);
        }

		/// <summary>
		/// Creates a sequential mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">List of input/output matrix tuples</param>
        protected IMiniBatch GetSequentialMiniBatch(uint[] rows, (Matrix<float> Input, Matrix<float>? Output)[] data)
        {
            List<Vector<float>>? temp;
            var inputData = new Dictionary<uint, List<Vector<float>>>();
            var outputData = new Dictionary<uint, List<Vector<float>>>();

            foreach (var (input, output) in data) {
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
                IGraphData? output = null;
                if (outputData.TryGetValue(item.Key, out temp))
                    output = new MatrixGraphData(_lap.CreateMatrixFromRows(temp));
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, new MatrixGraphData(input), output);
            }
            return miniBatch;
        }
    }
}
