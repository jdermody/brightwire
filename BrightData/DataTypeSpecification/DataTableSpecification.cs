using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTypeSpecification
{
    class DataTableSpecification : DataTypeSpecificationBase<IDataTable>
    {
        public DataTableSpecification(IDataTable dataTable) : base(
            dataTable.MetaData.GetName(), 
            DataSpecificationType.Composite,
            false,
            dataTable.ColumnTypes.Zip(dataTable.AllColumnsMetaData())
                .Select(ct => ct.First.AsDataFieldSpecification(ct.Second.GetName()))
                .ToArray()
        )
        {
        }
    }
}
