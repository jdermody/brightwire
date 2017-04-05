using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class MiniBatchGraphInput : IGraphInput
    {
        readonly IMiniBatchProvider _provider;
        readonly List<IWire> _target = new List<IWire>();

        public MiniBatchGraphInput(IMiniBatchProvider provider)
        {
            _provider = provider;
        }

        public int InputSize { get { return _provider.DataSource.InputSize; } }
        public int OutputSize { get { return _provider.DataSource.OutputSize; } }
        public int RowCount { get { return _provider.DataSource.RowCount; } }

        public void AddTarget(IWire target)
        {
            _target.Add(target);
        }

        public double? Train(ILearningContext context)
        {
            var miniBatchTrainingError = new List<double>();

            foreach (var miniBatch in _provider.GetMiniBatches(context.BatchSize, context.LinearAlgebraProvider.IsStochastic)) {
                var wireContext = new WireContext(context, miniBatch.Item2, true);
                foreach (var target in _target)
                    target.Send(miniBatch.Item1, wireContext);
                context.ApplyUpdates();

                if(wireContext.TrainingError.HasValue)
                    miniBatchTrainingError.Add(wireContext.TrainingError.Value);
            }

            double? trainingError = null;
            if(miniBatchTrainingError.Any())
                trainingError = miniBatchTrainingError.Average();

            return trainingError;
        }

        public IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Test(int batchSize = 128)
        {
            var ret = new List<(IIndexableVector, IIndexableVector)>();
            foreach (var miniBatch in _provider.GetMiniBatches(batchSize, false)) {
                foreach (var target in _target) {
                    var output = target.Send(miniBatch.Item1, null);
                    ret.AddRange(output.AsIndexable().Rows.Zip(miniBatch.Item2.AsIndexable().Rows, (a, t) => (a, t)));
                }
            }
            return ret;
        }

        public IReadOnlyList<IIndexableVector> Execute(int batchSize = 128)
        {
            var ret = new List<IIndexableVector>();
            foreach (var miniBatch in _provider.GetMiniBatches(batchSize, false)) {
                foreach (var target in _target) {
                    var output = target.Send(miniBatch.Item1, null);
                    ret.AddRange(output.AsIndexable().Rows);
                }
            }
            return ret;
        }
    }
}
