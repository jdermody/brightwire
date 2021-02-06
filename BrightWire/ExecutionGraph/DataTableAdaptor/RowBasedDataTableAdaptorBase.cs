using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Base class for data tables that work with data table rows
    /// </summary>
    public abstract class RowBasedDataTableAdaptorBase : DataTableAdaptorBase<object[]>
    {
	    /// <inheritdoc />
	    protected RowBasedDataTableAdaptorBase(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint[]? featureColumns) 
            : base(lap, dataTable, featureColumns)
        {
            // read the entire data table into memory
            dataTable.ForEachRow(row => _data.Add(row));
        }
    }
}
