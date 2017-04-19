using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class MiniBatchGraphInput : IGraphInput
    {
        readonly int _inputSize, _outputSize, _rowCount;
        readonly ILinearAlgebraProvider _lap;
        readonly List<IWire> _target = new List<IWire>();
        readonly List<ISecondaryInput> _secondary = new List<ISecondaryInput>();
        readonly int _channel;
        readonly bool _isSequential;

        public MiniBatchGraphInput(IDataSource dataSource, ILinearAlgebraProvider lap, int channel = 0)
        {
            _isSequential = dataSource.IsSequential;
            _channel = channel;
            _inputSize = dataSource.InputSize;
            _outputSize = dataSource.OutputSize;
            _rowCount = dataSource.RowCount;
            _lap = lap;
        }

        public bool IsSequential => _isSequential;
        public int InputSize => _inputSize;
        public int OutputSize => _outputSize;
        public int RowCount => _rowCount;
        public void AddSecondary(ISecondaryInput input) => _secondary.Add(input);

        public void AddTarget(IWire target)
        {
            _target.Add(target);
        }

        public double? Train(IMiniBatchProvider provider, ILearningContext context)
        {
            var batchList = new List<BatchContext>();
            var executionContext = new ExecutionContext(_lap);
            var miniBatches = provider.GetMiniBatches(context.BatchSize, context.LinearAlgebraProvider.IsStochastic, miniBatch => {
                var batchContext = new BatchContext(executionContext, context, miniBatch);
                _Execute(miniBatch, batchContext);
                batchList.Add(batchContext);
            });

            executionContext.Add(miniBatches);
            _ExecuteAll(executionContext);

            return batchList.Select(b => b.TrainingError).Average();
        }

        public IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Test(IMiniBatchProvider provider, int batchSize = 128)
        {
            var ret = new List<BatchContext>();
            var executionContext = new ExecutionContext(_lap);

            executionContext.Add(provider.GetMiniBatches(batchSize, false, miniBatch => {
                var batchContext = new BatchContext(executionContext, miniBatch);
                _Execute(miniBatch, batchContext);
                ret.Add(batchContext);
            }));
            _ExecuteAll(executionContext);

            return ret
                .SelectMany(bc => bc.Results)
                .SelectMany(r => r.Output.AsIndexable().Rows.Zip(r.Target.AsIndexable().Rows, (a, t) => (a, t)))
                .ToList()
            ;
        }

        public IReadOnlyList<IIndexableVector> Execute(IMiniBatchProvider provider, int batchSize = 128)
        {
            var executionContext = new ExecutionContext(_lap);
            var batchList = new List<BatchContext>();

            executionContext.Add(provider.GetMiniBatches(batchSize, false, miniBatch => {
                var batchContext = new BatchContext(executionContext, miniBatch);
                _Execute(miniBatch, batchContext);
                batchList.Add(batchContext);
            }));
            _ExecuteAll(executionContext);

            return batchList
                .SelectMany(bc => bc.Results)
                .SelectMany(bc => bc.Output.AsIndexable().Rows)
                .ToList()
            ;
        }

        class ApplyUpdates : IBackpropagation
        {
            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                context.LearningContext.ApplyUpdates();
            }

            public void Dispose()
            {
                // nop
            }
        }

        class EndOfSequenceBackpropagation : IBackpropagation
        {
            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                // nop
            }

            public void Dispose()
            {
                // nop
            }
        }

        void _Execute(IMiniBatch miniBatch, BatchContext context)
        {
            // notify any secondary inputs that we are starting
            foreach (var item in _secondary)
                item.OnStart(context);

            // process this batch
            if (context.IsTraining) {
                context.LearningContext.Log(writer => {
                    writer.WriteStartElement("mini-batch");
                    writer.WriteAttributeString("size", miniBatch.BatchSize.ToString());
                    writer.WriteRaw(miniBatch.CurrentSequence.Input.AsIndexable().AsXml);
                });
                context.RegisterBackpropagation(new ApplyUpdates(), 0);
            }
            while (true) {
                // send to all targets
                context.LearningContext?.Log(writer => writer.WriteStartElement("execution"));

                foreach (var target in _target)
                    target.Send(miniBatch.CurrentSequence.Input, _channel, context);

                context.LearningContext?.Log(writer => {
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                });

                // process the next item in the sequence (if any)
                if (miniBatch.HasNextSequence) {
                    miniBatch.GetNextSequence();

                    if (context.IsTraining) {
                        context.LearningContext.Log(writer => {
                            writer.WriteStartElement("next-item-in-sequence");
                            writer.WriteAttributeString("index", miniBatch.CurrentSequence.SequenceIndex.ToString());
                            writer.WriteRaw(miniBatch.CurrentSequence.Input.AsIndexable().AsXml);
                        });
                        foreach(var channel in context.ActiveChannels)
                            context.RegisterBackpropagation(new EndOfSequenceBackpropagation(), channel);
                    }

                    // notify secondary inputs
                    foreach (var item in _secondary)
                        item.OnNext(context);
                } else
                    break;
            }

            // apply updates
            if (context.IsTraining)
                context.Backward();
        }

        void _ExecuteAll(ExecutionContext context)
        {
            IGraphOperation next;
            while ((next = context.GetNextOperation()) != null)
                next.Execute();
        }
    }
}
