using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Operations
{
    internal abstract class OperationBase<T>  : IOperation<T>
    {
        readonly uint _stepsTotal;
        readonly string? _msg;

        public OperationBase(uint stepsTotal, string? msg)
        {
            _stepsTotal = stepsTotal;
            _msg = msg;
        }

        protected abstract void NextStep(uint index);
        protected abstract T GetResult(bool wasCancelled);

        public T Complete(INotifyUser? notifyUser, CancellationToken cancellationToken)
        {
            if (notifyUser is not null) {
                var id = Guid.NewGuid().ToString("n");
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
                
            }

            return GetResult(cancellationToken.IsCancellationRequested);
        }

        public virtual void Dispose()
        {
        }
    }
}
