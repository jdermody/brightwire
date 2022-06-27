using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Operations
{
    internal class CopyToConsumerOperation<T> : OperationBase<bool> where T: notnull
    {
        readonly ICanEnumerateDisposable<T> _columnData;
        readonly IConsumeColumnData<T>      _consumer;
        readonly IEnumerator<T>             _enumerator;

        public CopyToConsumerOperation(
            uint columnCount, 
            ICanEnumerateDisposable<T> columnData, 
            IConsumeColumnData<T> consumer
        ) : base(columnCount, null)
        {
            _columnData = columnData;
            _consumer = consumer;
            _enumerator = columnData.EnumerateTyped().GetEnumerator();
        }

        public override void Dispose()
        {
            _columnData.Dispose();
        }

        protected override void NextStep(uint index)
        {
            _enumerator.MoveNext();
            _consumer.Add(_enumerator.Current);
        }

        protected override bool GetResult(bool wasCancelled) => !wasCancelled;
    }
}
