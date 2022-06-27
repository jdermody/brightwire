using System.Linq;
using BrightData.DataTable2;

namespace BrightData.DataTypeSpecification
{
    class DataTableSpecification : DataTypeSpecificationBase<IDataTable>
    {
        public DataTableSpecification(BrightDataTable dataTable) : base(
            dataTable.TableMetaData.GetName(), 
            DataSpecificationType.Composite,
            false,
            dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData)
                .Select(ct => ct.First.AsDataFieldSpecification(ct.Second.GetName()))
                .ToArray()
        )
        {
        }
    }
}
