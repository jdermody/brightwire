using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Operation
{
    internal class NopOperation : IOperation
    {
        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            notify?.OnStartOperation(id, msg);
            notify?.OnCompleteOperation(id, ct.IsCancellationRequested);
            return Task.CompletedTask;
        }
    }
}
