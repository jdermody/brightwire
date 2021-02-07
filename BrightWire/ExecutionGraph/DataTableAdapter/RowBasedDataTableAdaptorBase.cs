using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Base class for data tables that work with data table rows
    /// </summary>
    public abstract class RowBasedDataTableAdapterBase : DataTableAdapterBase<object[]>
    {
	    /// <inheritdoc />
	    protected RowBasedDataTableAdapterBase(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint[] featureColumns) 
            : base(lap, dataTable, featureColumns)
        {
            // read the entire data table into memory
            dataTable.ForEachRow(row => _data.Add(row));
        }
    }
}
