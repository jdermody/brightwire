using System;

namespace BrightData.DataTable.Meta
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
                throw new ArgumentException($"Failed constraint validation: {item}", nameof(item));
            return true;
        }
    }
}
