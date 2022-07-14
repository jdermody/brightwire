using System.Collections.Generic;

namespace BrightData.DataTable.Operations
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
            _enumerator = columnData.Values.GetEnumerator();
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
