using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.ConstraintValidation
{
    /// <summary>
    /// Throws an exception if a constraint fails validation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate"></param>
    internal class ThrowOnInvalidConstraint<T>(ThrowOnInvalidConstraint<T>.Predicate predicate) : IConstraintValidator<T>
        where T : notnull
    {
        public delegate bool Predicate(in T value);

        public bool Allow(in T item)
        {
            if (!predicate(item))
                throw new Exception($"Failed constraint validation: {item}");
            return true;
        }
    }
}
