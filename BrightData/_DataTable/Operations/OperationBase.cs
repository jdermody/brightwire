using System;
using System.Threading;

namespace BrightData.DataTable.Operations
{
    internal abstract class OperationBase<T> : IOperation<T>
    {
        readonly uint _stepsTotal;
        readonly string? _msg;
        readonly bool _isNop;

        protected OperationBase(uint stepsTotal, string? msg, bool isNop = false)
        {
            _stepsTotal = stepsTotal;
            _msg = msg;
            _isNop = isNop;
        }

        protected abstract void NextStep(uint index);
        protected abstract T GetResult(bool wasCancelled);

        public T Complete(INotifyUser? notifyUser, CancellationToken cancellationToken)
        {
            if (!_isNop) {
                if (notifyUser is not null) {
                    var id = Guid.NewGuid();
                    notifyUser.OnStartOperation(id, _msg);
                    var total = (float)_stepsTotal;
                    var prevProgress = -1;
                    for (uint i = 0; i < _stepsTotal && !cancellationToken.IsCancellationRequested; i++) {
                        NextStep(i);
                        var progress = i / total;
                        var progressPercent = (int)(progress * 100);
                        if (progressPercent > prevProgress) {
                            notifyUser.OnOperationProgress(id, progress);
                            prevProgress = progressPercent;
                        }
                    }

                    notifyUser.OnCompleteOperation(id, cancellationToken.IsCancellationRequested);
                }
                else {
                    for (uint i = 0; i < _stepsTotal && !cancellationToken.IsCancellationRequested; i++)
                        NextStep(i);
                }
            }

            return GetResult(cancellationToken.IsCancellationRequested);
        }

        public virtual void Dispose()
        {
        }
    }
}
