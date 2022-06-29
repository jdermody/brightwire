using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Operations
{
    internal class NopMetaDataOperation : OperationBase<(uint, MetaData)>
    {
        readonly uint _columnIndex;
        readonly MetaData _metaData;

        public NopMetaDataOperation(uint columnIndex, MetaData metaData) : base(0, null, true)
        {
            _columnIndex = columnIndex;
            _metaData = metaData;
        }

        protected override void NextStep(uint index)
        {
            throw new NotImplementedException();
        }

        protected override (uint, MetaData) GetResult(bool wasCancelled) => (_columnIndex, _metaData);
    }
}
