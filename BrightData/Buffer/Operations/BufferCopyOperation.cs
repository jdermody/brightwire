using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    /// <summary>
    /// Adds each item in the buffer to a destination
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="from">Buffer to process</param>
    /// <param name="to">Destination</param>
    /// <param name="onComplete">Optional callback on completion</param>
    internal class BufferCopyOperation<T>(IReadOnlyBuffer<T> from, IAppendBlocks<T> to, Action? onComplete)
        : IOperation
        where T : notnull
    {
        public async Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            if (notify is not null)
                await from.ForEachWithProgressNotification(notify, to.Append, msg, ct);
            else
                await from.ForEachBlock(to.Append, ct);
            onComplete?.Invoke();
        }
    }
}
