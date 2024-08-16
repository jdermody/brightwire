using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.DataTable.Meta
{
    /// <summary>
    /// Data type specification base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="canRepeat"></param>
    /// <param name="children"></param>
    abstract class DataTypeSpecificationBase<T>(string? name, DataSpecificationType type, bool canRepeat, IDataTypeSpecification[]? children)
        : IDataTypeSpecification<T>
        where T : notnull
    {
        readonly List<Predicate<T>> _predicates = [];

        public string? Name { get; } = name;
        public IDataTypeSpecification[]? Children { get; } = children;
        public Type UnderlyingType { get; } = typeof(T);
        public DataSpecificationType SpecificationType { get; } = type;
        public bool IsValid(T instance) => instance.GetType() == UnderlyingType && (_predicates.Count == 0 || _predicates.All(p => p(instance)));
        public void AddPredicate(Predicate<T> predicate) => _predicates.Add(predicate);
        public bool CanRepeat { get; } = canRepeat;
    }
}
