using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    /// <summary>
    /// Notification from an aggregate operation
    /// </summary>
    /// <param name="count"></param>
    /// <param name="notify"></param>
    /// <param name="msg"></param>
    /// <param name="ct"></param>
    class AggregateNotification(int count, INotifyOperationProgress notify, string? msg, CancellationToken ct)
        : INotifyOperationProgress
    {
        readonly Guid                    _id = Guid.NewGuid();
        readonly Dictionary<Guid, float> _taskProgress = new();
        readonly HashSet<Guid>           _completed = [];
        int                              _progressNotifications = 0;

        public void OnStartOperation(Guid operationId, string? msg1 = null)
        {
            _taskProgress.Add(operationId, 0f);
            if(_taskProgress.Count == count)
                notify.OnStartOperation(_id, msg);
        }

        public void OnOperationProgress(Guid operationId, float progressPercent)
        {
            _taskProgress[operationId] = progressPercent;
            if (++_progressNotifications % count == 0) {
                var aggregateProgress = _taskProgress.Values.Average();
                notify.OnOperationProgress(_id, aggregateProgress);
            }
        }

        public void OnCompleteOperation(Guid operationId, bool wasCancelled)
        {
            _taskProgress[operationId] = 1f;
            if(_completed.Add(operationId) && _completed.Count == count)
                notify.OnCompleteOperation(_id, ct.IsCancellationRequested);
        }

        public void OnMessage(string _)
        {
            // ignore
        }
    }

    /// <summary>
    /// Aggregate operations run multiple operations as one concurrently
    /// </summary>
    /// <param name="operations"></param>
    internal class AggregateOperation(IOperation[] operations) : IOperation
    {
        public Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            if (notify != null)
                notify = new AggregateNotification(operations.Length, notify, msg, ct);
            return Task.WhenAll(operations.Select(x => x.Execute(notify, null, ct)));
        }
    }
}
