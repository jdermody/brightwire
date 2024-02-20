namespace BrightData.DataTable.ConstraintValidation
{
    /// <summary>
    /// Field specification
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="canRepeat"></param>
    class FieldSpecification<T>(string? name, bool canRepeat = false) : DataTypeSpecificationBase<T>(name, DataSpecificationType.Field, canRepeat, null) where T : notnull;
}
