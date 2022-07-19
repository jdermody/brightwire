using System.Collections.Generic;
using BrightData.Buffer.Hybrid;

namespace BrightData.DataTable.Operations
{
    internal class ColumnConversionOperation<FT, TT> : OperationBase<ITypedSegment?> where FT : notnull where TT : notnull
    {
        readonly ICanEnumerateDisposable<FT> _input;
        readonly IConvertColumn<FT, TT>      _converter;
        readonly HybridBufferSegment<TT>     _outputBuffer;
        readonly IEnumerator<FT>             _enumerator;

        public ColumnConversionOperation(
            uint rowCount,
            ICanEnumerateDisposable<FT> input, 
            IConvertColumn<FT, TT> converter, 
            HybridBufferSegment<TT> outputBuffer
        ) : base(rowCount, null)
        {
            _input = input;
            _converter = converter;
            _outputBuffer = outputBuffer;
            _enumerator = _input.Values.GetEnumerator();
        }

        public override void Dispose()
        {
            _enumerator.Dispose();
            _input.Dispose();
            _outputBuffer.Dispose();
        }

        protected override void NextStep(uint index)
        {
            _enumerator.MoveNext();
            _converter.Convert(_enumerator.Current, _outputBuffer, index);
        }

        protected override ITypedSegment? GetResult(bool wasCancelled)
        {
            if (wasCancelled)
                return null;

            var segment = (ITypedSegment) _outputBuffer;
            segment.MetaData.SetType(segment.SegmentType);
            if(segment.SegmentType.IsNumeric())
                segment.MetaData.Set(Consts.IsNumeric, true);
            _converter.Finalise(segment.MetaData);
            return segment;
        }
    }
}
