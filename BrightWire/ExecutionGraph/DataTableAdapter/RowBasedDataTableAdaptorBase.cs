using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Base class for data tables that work with data table rows
    /// </summary>
    public abstract class RowBasedDataTableAdapterBase : DataTableAdapterBase<ICanRandomlyAccessData>
    {
        /// <inheritdoc />
	    protected RowBasedDataTableAdapterBase(IDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
        }

        /// <summary>
        /// Returns a tensor segment from a segment provider
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="segmentProvider"></param>
        /// <returns></returns>
        protected IReadOnlyNumericSegment<float> GetSegment(uint columnIndex, ICanRandomlyAccessData segmentProvider)
        {
            return ((IHaveReadOnlyTensorSegment<float>)segmentProvider[columnIndex]).ReadOnlySegment;
        }

        /// <inheritdoc />
        protected override IEnumerable<ICanRandomlyAccessData> GetRows(uint[] rows) => _dataTable.GetRows(rows).Result.Cast<ICanRandomlyAccessData>();
    }
}
