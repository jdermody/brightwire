using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Table.Operation.Helper;

namespace BrightData.Table.Operation.Conversion
{
    internal class OneHotVectoriser<T> : ConversionBase<T, ReadOnlyVector> where T: notnull
    {
        readonly ICanIndex<T> _indexer;

        public OneHotVectoriser(IReadOnlyBuffer<T> input, ICanIndex<T> indexer, IAppendToBuffer<ReadOnlyVector> output) : base(input, output)
        {
            _indexer = indexer;
        }

        protected override ReadOnlyVector Convert(T from)
        {
            var index = _indexer.GetIndex(from);
            return new ReadOnlyVector(_indexer.Size, x => x == index ? 1f : 0f);
        }
    }
}
