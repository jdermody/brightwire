using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightWire.ExecutionGraph.Helper;
using BrightData;
using BrightData.DataTable;

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
        protected readonly IDataTable _dataTable;

        /// <summary>
	    /// Constructor
	    /// </summary>
        /// <param name="dataTable"></param>
	    /// <param name="featureColumns"></param>
	    protected DataTableAdapterBase(IDataTable dataTable, uint[] featureColumns)
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
        public abstract Task<MiniBatch> Get(uint[] rows);

        /// <inheritdoc />
        public abstract IDataSource CloneWith(IDataTable dataTable);

        /// <inheritdoc />
        public VectorisationModel? InputVectoriser { get; protected set; } = null;

        /// <inheritdoc />
        public VectorisationModel? OutputVectoriser { get; protected set; } = null;

        /// <inheritdoc />
        public virtual uint[][] GetSequentialBatches()
        {
            return [
                RowCount.AsRange().ToArray()
            ];
        }

        /// <summary>
        /// Returns data for the specified row indices
        /// </summary>
        /// <param name="rows">List of row indices</param>
        protected abstract IAsyncEnumerable<T> GetRows(uint[] rows);

        /// <summary>
		/// Creates a mini batch
		/// </summary>
		/// <param name="rows">Row indices</param>
		/// <param name="data">Array of input/output pairs</param>
        protected MiniBatch GetMiniBatch(uint[] rows, (float[] Input, float[] Output)[] data)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            var input = lap.CreateMatrix((uint)data.Length, InputSize, (x, y) => data[x].Input[y]).AsGraphData();
            var output = OutputSize > 0 
                ? lap.CreateMatrix((uint)data.Length, (uint)OutputSize, (x, y) => data[x].Output[y]).AsGraphData()
                : null;

            return new MiniBatch(rows, this, input, output);
        }
    }
}
