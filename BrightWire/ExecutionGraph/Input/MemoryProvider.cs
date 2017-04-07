using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class MemoryProvider : ISecondaryInput, IBackpropagation
    {
        readonly float[] _data;
        readonly ILinearAlgebraProvider _lap;
        readonly IWire _sendTo;
        readonly int _channel;

        public MemoryProvider(IGraphInput graphInput, IWire sendTo, ILinearAlgebraProvider lap, int channel)
        {
            _lap = lap;
            _data = new float[sendTo.InputSize];
            _channel = channel;
            _sendTo = sendTo;

            graphInput.AddSecondary(this);
        }

        public void OnStart(IBatchContext context)
        {
            if(context.Batch.IsSequential) {
                if (context.IsTraining)
                    context.RegisterBackpropagation(this, _channel);
                var memory = GetMemory(context.Batch.BatchSize);
                _sendTo.Send(memory, _channel, context);
            }
        }

        public void OnNext(IBatchContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_channel);
            _sendTo.Send(memory, _channel, context);
        }

        public IMatrix GetMemory(int batchSize) => _lap.Create(batchSize, _data.Length, (x, y) => _data[y]);

        public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
        {
            using (var columnSums = errorSignal.ColumnSums()) {
                var initialDelta = columnSums.AsIndexable();
                for (var j = 0; j < _data.Length; j++)
                    _data[j] += initialDelta[j] * context.LearningContext.LearningRate;
            }
        }

        public void Dispose()
        {
            // nop
        }
    }
}
