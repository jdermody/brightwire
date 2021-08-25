using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTypeSpecification
{
    abstract class DataTypeSpecificationBase<T> : IDataTypeSpecification<T> where T: notnull
    {
        readonly List<Predicate<T>> _predicates = new();

        protected DataTypeSpecificationBase(string? name, DataSpecificationType type, bool canRepeat, IDataTypeSpecification[]? children)
        {
            Children = children;
            Name = name;
            SpecificationType = type;
            CanRepeat = canRepeat;
        }

        public string? Name { get; }
        public IDataTypeSpecification[]? Children { get; }
        public Type UnderlyingType { get; } = typeof(T);
        public DataSpecificationType SpecificationType { get; }
        public bool IsValid(T instance) => instance.GetType() == UnderlyingType && (_predicates.Count == 0 || _predicates.All(p => p(instance)));
        public void AddPredicate(Predicate<T> predicate) => _predicates.Add(predicate);
        public bool CanRepeat { get; }
    }
}
