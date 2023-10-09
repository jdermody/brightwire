using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class OneHotConverter<T> : ReadOnlyConverterBase<T, ReadOnlyVector> where T: notnull
    {
        readonly ICanIndex<T> _indexer;

        public OneHotConverter(IReadOnlyBuffer<T> from, ICanIndex<T> indexer) : base(from)
        {
            _indexer = indexer;
        }

        protected override ReadOnlyVector Convert(in T from)
        {
            var index = _indexer.GetIndex(from);
            return new ReadOnlyVector(_indexer.Size, x => x == index ? 1f : 0f);
        }
    }
}
