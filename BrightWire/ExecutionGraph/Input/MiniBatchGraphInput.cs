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

        public event Action<IBatchContext> OnSequenceStart;
        public event Action<IBatchContext> OnSequenceContinue;
        public event Action<IBatchContext> OnSequenceEnd;

        public bool IsSequential => _isSequential;
        public int InputSize => _inputSize;
        public int OutputSize => _outputSize;
        public int RowCount => _rowCount;

        public void AddTarget(IWire target)
        {
            _target.Add(target);
        }

        public double? Train(IMiniBatchProvider provider, ILearningContext context)
        {
            var batchList = new List<BatchContext>();
            var executionContext = new ExecutionContext(_lap);

            var isSequential = false;
            executionContext.Add(provider.GetMiniBatches(context.BatchSize, context.LinearAlgebraProvider.IsStochastic, miniBatch => {
                var batchContext = new BatchContext(executionContext, context, miniBatch.Input.RowCount, miniBatch.Output);
                if (miniBatch.Type == MiniBatchType.SequenceStart) {
                    isSequential = true;
                    OnSequenceStart?.Invoke(batchContext);
                }else if(isSequential)
                    OnSequenceContinue?.Invoke(batchContext);

                foreach (var target in _target)
                    target.Send(miniBatch.Input, _channel, batchContext);

                if (!isSequential || miniBatch.Type == MiniBatchType.SequenceEnd) {
                    OnSequenceEnd?.Invoke(batchContext);
                    context.ApplyUpdates();
                    isSequential = false;
                }

                batchList.Add(batchContext);
            }));
            _Execute(executionContext);

            var miniBatchTrainingError = new List<double>();
            foreach (var item in batchList) {
                if (item.TrainingError.HasValue)
                    miniBatchTrainingError.Add(item.TrainingError.Value);
            }

            // calculate the epoch training error
            double? trainingError = null;
            if(miniBatchTrainingError.Any())
                trainingError = miniBatchTrainingError.Average();

            return trainingError;
        }

        public IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Test(IMiniBatchProvider provider, int batchSize = 128)
        {
            var ret = new List<BatchContext>();
            var executionContext = new ExecutionContext(_lap);
            var isSequential = false;

            executionContext.Add(provider.GetMiniBatches(batchSize, false, miniBatch => {
                var batchContext = new BatchContext(executionContext, miniBatch.Input.RowCount, miniBatch.Output);
                if (miniBatch.Type == MiniBatchType.SequenceStart) {
                    OnSequenceStart?.Invoke(batchContext);
                    isSequential = true;
                } else if (isSequential)
                    OnSequenceContinue?.Invoke(batchContext);

                foreach (var target in _target)
                    target.Send(miniBatch.Input, _channel, batchContext);

                if (miniBatch.Type == MiniBatchType.SequenceEnd)
                    OnSequenceEnd?.Invoke(batchContext);
                ret.Add(batchContext);
            }));
            _Execute(executionContext);
            return ret
                .SelectMany(bc => bc.Output.AsIndexable().Rows.Zip(bc.Target.AsIndexable().Rows, (a, t) => (a, t)))
                .ToList()
            ;
        }

        public IReadOnlyList<IIndexableVector> Execute(IMiniBatchProvider provider, int batchSize = 128)
        {
            var executionContext = new ExecutionContext(_lap);
            var batchList = new List<BatchContext>();
            var isSequential = false;

            executionContext.Add(provider.GetMiniBatches(batchSize, false, miniBatch => {
                var batchContext = new BatchContext(executionContext, miniBatch.Input.RowCount);
                if (miniBatch.Type == MiniBatchType.SequenceStart) {
                    OnSequenceStart?.Invoke(batchContext);
                    isSequential = true;
                } else if (isSequential)
                    OnSequenceContinue?.Invoke(batchContext);

                foreach (var target in _target)
                    target.Send(miniBatch.Input, _channel, batchContext);

                if (miniBatch.Type == MiniBatchType.SequenceEnd)
                    OnSequenceEnd?.Invoke(batchContext);
                batchList.Add(batchContext);
            }));
            _Execute(executionContext);

            return batchList
                .SelectMany(bc => bc.Output.AsIndexable().Rows)
                .ToList()
            ;
        }

        void _Execute(ExecutionContext context)
        {
            IGraphOperation next;
            while ((next = context.GetNextOperation()) != null)
                next.Execute();
        }
    }
}
