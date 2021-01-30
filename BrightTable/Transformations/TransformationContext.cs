using BrightData;

namespace BrightTable.Transformations
{
    internal class TransformationContext<TF, TT> : ITransformationContext
        where TF: notnull
        where TT: notnull
    {
        readonly IDataTableSegment<TF> _column;
        readonly ITransformColumn<TF, TT> _converter;
        readonly IHybridBuffer<TT> _buffer;

        public TransformationContext(IDataTableSegment<TF> column, ITransformColumn<TF, TT> converter, IHybridBuffer<TT> buffer)
        {
            _column = column;
            _converter = converter;
            _buffer = buffer;
        }

        public uint Transform()
        {
            uint ret = 0;
            foreach (var item in _column.EnumerateTyped()) {
                if (_converter.Convert(item, _buffer))
                    ++ret;
            }
            var columnMetadata = _column.MetaData;
            var segment = (ISingleTypeTableSegment) _buffer;
            var metaData = segment.MetaData;
            columnMetadata.CopyTo(metaData);
            metaData.SetType(segment.SingleType);
            _converter.Finalise(metaData);
            return ret;
        }

        public IHybridBuffer Buffer => _buffer;
    }
}
