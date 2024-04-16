using System.Linq;

namespace BrightData.DataTable.ConstraintValidation
{
    /// <summary>
    /// Data table specification
    /// </summary>
    /// <param name="dataTable"></param>
    class DataTableSpecification(IDataTable dataTable) : DataTypeSpecificationBase<IDataTable>(
        dataTable.MetaData.GetName(),
        DataSpecificationType.Composite,
        false,
        dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData)
            .Select(ct => ct.First.AsDataFieldSpecification(ct.Second.GetName()))
            .ToArray()
    );
}
