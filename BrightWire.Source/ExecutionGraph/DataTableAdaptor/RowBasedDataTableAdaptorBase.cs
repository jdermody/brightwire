namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Base class for data tables that work with data table rows
    /// </summary>
    abstract class RowBasedDataTableAdaptorBase : DataTableAdaptorBase<IRow>
    {
        public RowBasedDataTableAdaptorBase(ILinearAlgebraProvider lap, IDataTable dataTable) 
            : base(lap, dataTable)
        {
            // read the entire data table into memory
            dataTable.ForEach(row => _data.Add(row));
        }
    }
}
