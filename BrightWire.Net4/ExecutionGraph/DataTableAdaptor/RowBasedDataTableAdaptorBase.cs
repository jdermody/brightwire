using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
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
