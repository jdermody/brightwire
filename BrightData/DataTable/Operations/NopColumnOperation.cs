using System;

namespace BrightData.DataTable.Operations
{
    internal class NopColumnOperation : OperationBase<ISingleTypeTableSegment?>
    {
        readonly ISingleTypeTableSegment _column;

        public NopColumnOperation(ISingleTypeTableSegment column) : base(0, null, true)
        {
            _column = column;
        }

        protected override void NextStep(uint index)
        {
            throw new NotImplementedException();
        }

        protected override ISingleTypeTableSegment? GetResult(bool wasCancelled) => wasCancelled ? null : _column;
    }
}
