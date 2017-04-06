using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class MemoryProvider : IBackpropagation
    {
        readonly float[] _data;
        readonly ILinearAlgebraProvider _lap;
        readonly IWire _sendTo;
        readonly int _channel;

        //class LoadMemory : IGraphOperation
        //{
        //    readonly MemoryProvider _memoryProvider;
        //    readonly IBatchContext _context;

        //    public LoadMemory(MemoryProvider memoryProvider, IBatchContext context)
        //    {
        //        _context = context;
        //        _memoryProvider = memoryProvider;
        //    }

        //    public bool CanContinue => true;

        //    public void Execute()
        //    {
        //        _memoryProvider._sendTo.Send(_memoryProvider.GetMemory(_context.BatchSize), _memoryProvider._channel, _context);
        //    }
        //}

        public MemoryProvider(IGraphInput graphInput, IWire sendTo, ILinearAlgebraProvider lap, int channel)
        {
            _lap = lap;
            _data = new float[sendTo.InputSize];
            _channel = channel;
            _sendTo = sendTo;

            graphInput.OnSequenceStart += bc => {
                if(bc.IsTraining)
                    bc.AddBackpropagation(this, channel);
                //bc.ExecutionContext.Add(new LoadMemory(this, bc));
                _sendTo.Send(GetMemory(bc.BatchSize), _channel, bc);
            };
            graphInput.OnSequenceContinue += bc => {
                var memory = bc.ExecutionContext.GetMemory(_channel);
                _sendTo.Send(memory, _channel, bc);
            };
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
