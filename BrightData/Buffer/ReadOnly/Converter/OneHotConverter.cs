using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts via a one hot encoder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="from"></param>
    /// <param name="indexer"></param>
    internal class OneHotConverter<T>(IReadOnlyBuffer<T> from, ICanIndex<T> indexer) : ReadOnlyConverterBase<T, ReadOnlyVector>(from)
        where T : notnull
    {
        protected override ReadOnlyVector Convert(in T from)
        {
            var index = indexer.GetIndex(from);
            return new ReadOnlyVector(indexer.Size, x => x == index ? 1f : 0f);
        }
    }
}
