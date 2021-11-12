using System.Linq;

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
