using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    /// <summary>
    /// An operation that does nothing
    /// </summary>
    internal class NopOperation : IOperation
    {
        public Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            notify?.OnStartOperation(id, msg);
            notify?.OnCompleteOperation(id, ct.IsCancellationRequested);
            return Task.CompletedTask;
        }
    }
}
