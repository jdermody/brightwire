using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Operation
{
    class AggregateNotification : INotifyUser
    {
        readonly int _count;
        readonly Guid _id = Guid.NewGuid();
        readonly Dictionary<Guid, float> _taskProgress = new();
        readonly HashSet<Guid> _completed = new();
        readonly INotifyUser _notify;
        readonly CancellationToken _ct;
        readonly string? _msg;
        int _progressNotifications = 0;

        public AggregateNotification(int count, INotifyUser notify, string? msg, CancellationToken ct)
        {
            _count = count;
            _notify = notify;
            _msg = msg;
            _ct = ct;
        }

        public void OnStartOperation(Guid operationId, string? msg = null)
        {
            _taskProgress.Add(operationId, 0f);
            if(_taskProgress.Count == _count)
                _notify.OnStartOperation(_id, _msg);
        }

        public void OnOperationProgress(Guid operationId, float progressPercent)
        {
            _taskProgress[operationId] = progressPercent;
            if (++_progressNotifications % _count == 0) {
                var aggregateProgress = _taskProgress.Values.Average();
                _notify.OnOperationProgress(_id, aggregateProgress);
            }
        }

        public void OnCompleteOperation(Guid operationId, bool wasCancelled)
        {
            _taskProgress[operationId] = 1f;
            if(_completed.Add(operationId) && _completed.Count == _count)
                _notify.OnCompleteOperation(_id, _ct.IsCancellationRequested);
        }

        public void OnMessage(string msg)
        {
            // ignore
        }
    }

    internal class AggregateOperation : IOperation
    {
        readonly IOperation[] _operations;

        public AggregateOperation(IOperation[] operations)
        {
            _operations = operations;
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            if (notify != null)
                notify = new AggregateNotification(_operations.Length, notify, msg, ct);
            return Task.WhenAll(_operations.Select(x => x.Process(notify, null, ct)));
        }
    }
}
