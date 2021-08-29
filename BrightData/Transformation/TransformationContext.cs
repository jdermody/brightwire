using System;
using System.Threading;

namespace BrightData.Transformation
{
    /// <summary>
    /// A transformation context reads values from a column and writes transformed values to a buffer
    /// </summary>
    /// <typeparam name="TF"></typeparam>
    /// <typeparam name="TT"></typeparam>
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

        public uint Transform(CancellationToken ct)
        {
            // write the transformed values
            uint ret = 0, index = 0;
            foreach (var item in _column.EnumerateTyped()) {
                if (ct.IsCancellationRequested)
                    break;

                var wasConverted = _converter.Convert(item, _buffer, index++);
                if (wasConverted)
                    ++ret;
            }

            // finalise
            var columnMetadata = _column.MetaData;
            var segment = (ISingleTypeTableSegment) _buffer;
            var metaData = segment.MetaData;
            columnMetadata.CopyTo(metaData, Consts.SimpleMetaData);
            metaData.SetType(segment.SingleType);
            if(segment.SingleType.IsNumeric())
                metaData.Set(Consts.IsNumeric, true);
            _converter.Finalise(metaData);
            return ret;
        }

        public IHybridBuffer Buffer => _buffer;
    }
}
