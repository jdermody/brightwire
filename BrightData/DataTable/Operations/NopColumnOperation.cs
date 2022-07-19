using System;

namespace BrightData.DataTable.Operations
{
    internal class NopColumnOperation : OperationBase<ITypedSegment?>
    {
        readonly ITypedSegment _column;

        public NopColumnOperation(ITypedSegment column) : base(0, null, true)
        {
            _column = column;
        }

        protected override void NextStep(uint index)
        {
            throw new NotImplementedException();
        }

        protected override ITypedSegment? GetResult(bool wasCancelled) => wasCancelled ? null : _column;
    }
}
