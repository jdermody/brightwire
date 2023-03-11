using System.Collections.Generic;
using BrightData;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Base class for data tables that work with data table rows
    /// </summary>
    public abstract class RowBasedDataTableAdapterBase : DataTableAdapterBase<ICanRandomlyAccessData>
    {
        /// <inheritdoc />
	    protected RowBasedDataTableAdapterBase(BrightDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
        }

        /// <summary>
        /// Returns a tensor segment from a segment provider
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="segmentProvider"></param>
        /// <returns></returns>
        protected ITensorSegment GetSegment(uint rowIndex, uint columnIndex, ICanRandomlyAccessData segmentProvider)
        {
            return ((IHaveTensorSegment)segmentProvider[columnIndex]).Segment;
        }

        /// <inheritdoc />
        protected override IEnumerable<ICanRandomlyAccessData> GetRows(uint[] rows) => _dataTable.GetRows(rows);
    }
}
