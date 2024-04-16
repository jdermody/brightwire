using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Buffer.ReadOnly.Converter
{
    internal class CastConverter<FT, TT> : ReadOnlyConverterBase<FT, TT> 
        where FT : notnull 
        where TT : notnull
    {
        public CastConverter(IReadOnlyBuffer<FT> from) : base(from)
        {
        }

        protected override void Convert(in FT from, ref TT to)
        {
            to = (TT)(object)from;
        }

        protected override TT Convert(in FT from)
        {
            return (TT)(object)from;
        }
    }
}
