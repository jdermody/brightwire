using System;

namespace BrightData.DataTable.Operations
{
    internal class NopColumnOperation : OperationBase<ITableSegment?>
    {
        readonly ITableSegment _column;

        public NopColumnOperation(ITableSegment column) : base(0, null, true)
        {
            _column = column;
        }

        protected override void NextStep(uint index)
        {
            throw new NotImplementedException();
        }

        protected override ITableSegment? GetResult(bool wasCancelled) => wasCancelled ? null : _column;
    }
}
