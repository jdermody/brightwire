using System;
using System.Collections.Generic;
using System.Text;
using BrightData;

namespace BrightTable.Transformations.Conversions
{
    class ConversionContext<TF, TT> : IColumnConversion
    {
        private readonly IDataTableSegment<TF> _column;
        private readonly IConvert<TF, TT> _converter;
        private readonly IAutoGrowBuffer<TT> _buffer;

        public ConversionContext(IDataTableSegment<TF> column, IConvert<TF, TT> converter, IAutoGrowBuffer<TT> buffer)
        {
            _column = column;
            _converter = converter;
            _buffer = buffer;
        }

        public uint Convert()
        {
            uint ret = 0;
            foreach (var item in _column.EnumerateTyped()) {
                if (_converter.Convert(item, _buffer))
                    ++ret;
            }

            var segment = (ISingleTypeTableSegment) _buffer;
            var metaData = segment.MetaData;
            metaData.Set(Consts.Type, segment.SingleType.ToString());
            _converter.Finalise(metaData);
            return ret;
        }

        public IAutoGrowBuffer Buffer => _buffer;
    }
}
