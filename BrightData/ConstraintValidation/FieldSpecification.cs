namespace BrightData.ConstraintValidation
{
    class FieldSpecification<T> : DataTypeSpecificationBase<T> where T : notnull
    {
        public FieldSpecification(string? name, bool canRepeat = false) : base(name, DataSpecificationType.Field, canRepeat, null)
        {
        }
    }
}
