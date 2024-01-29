using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Converter
{
    internal class CustomConversionFunction<FT, TT>(Func<FT, TT> converter) : ICanConvert<FT, TT> where FT : notnull where TT : notnull
    {
        public TT Convert(FT data) => converter(data);
        public Type From => typeof(FT);
        public Type To => typeof(TT);
    }
}
