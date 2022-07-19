using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
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

        protected ITensorSegment GetSegment(uint rowIndex, uint columnIndex, ICanRandomlyAccessData segment, LinearAlgebraProvider lap)
        {
            return ((IHaveTensorSegment)segment[columnIndex]).Segment;;
        }

        /// <inheritdoc />
        protected override IEnumerable<ICanRandomlyAccessData> GetRows(uint[] rows) => _dataTable.GetRows(rows);
    }
}
