using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Base class for data tables that work with data table rows
    /// </summary>
    public abstract class RowBasedDataTableAdapterBase : DataTableAdapterBase<IDataTableSegment>
    {
	    /// <inheritdoc />
	    protected RowBasedDataTableAdapterBase(IRowOrientedDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<IDataTableSegment> GetRows(uint[] rows) => _dataTable.Rows(rows);
    }
}
