using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Operations
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
