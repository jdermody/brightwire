using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.ConstraintValidation
{
    internal class ThrowOnInvalidConstraint<T> : IConstraintValidator<T> where T : notnull
    {
        public delegate bool Predicate(in T value);
        readonly Predicate _predicate;

        public ThrowOnInvalidConstraint(Predicate predicate)
        {
            _predicate = predicate;
        }

        public bool Allow(in T item)
        {
            if (!_predicate(item))
                throw new Exception($"Failed constraint validation: {item}");
            return true;
        }
    }
}
