using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Helper
{
    internal class MemoryFeeder : IBackpropagation
    {
        readonly float[] _data;
        readonly ILinearAlgebraProvider _lap;
        readonly int _channel;

        public MemoryFeeder(ILinearAlgebraProvider lap, int channel, float[] data)
        {
            _lap = lap;
            _data = data;
            _channel = channel;
        }

        public int Channel => _channel;

        public void Dispose()
        {
            // nop
        }

        //private void OnSignal(WireSignalEventArgs args)
        //{
        //    var context = args.Context;
        //    var batch = context.Batch;
        //    var sequence = batch.CurrentSequence;

        //    if (batch.IsSequential) {
        //        if (sequence.SequenceIndex == 0)
        //            _OnStart(context);
        //        else
        //            _OnNext(context);
        //    }
        //}

        public IMatrix OnStart(IBatchContext context)
        {
            if (context.IsTraining)
                context.RegisterBackpropagation(this, _channel);
            var memory = GetMemory(context.Batch.BatchSize);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("init-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            return memory;
        }

        public IMatrix OnNext(IBatchContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_channel);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("read-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            return memory;
        }

        public IMatrix GetMemory(int batchSize) => _lap.Create(batchSize, _data.Length, (x, y) => _data[y]);

        public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
        {
            context.LearningContext.Log(writer => {
                writer.WriteStartElement("memory-backpropagation");
                if (context.LearningContext.LogMatrixValues)
                    writer.WriteRaw(errorSignal.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            using (var columnSums = errorSignal.ColumnSums()) {
                var initialDelta = columnSums.AsIndexable();
                for (var j = 0; j < _data.Length; j++)
                    _data[j] += initialDelta[j] * context.LearningContext.LearningRate;
            }
        }
    }
}
