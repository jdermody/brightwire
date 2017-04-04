using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class MiniBatchFeeder : IGraphInput
    {
        readonly IMiniBatchProvider _provider;
        readonly List<IWire> _target = new List<IWire>();
        readonly bool _shouldBackpropagate;

        public MiniBatchFeeder(IMiniBatchProvider provider, bool shouldBackpropagate)
        {
            _provider = provider;
            _shouldBackpropagate = shouldBackpropagate;
        }

        public int InputSize { get { return _provider.DataSource.InputSize; } }
        public int OutputSize { get { return _provider.DataSource.OutputSize; } }

        public void AddTarget(IWire target)
        {
            _target.Add(target);
        }

        public double? Execute(ILearningContext context)
        {
            var miniBatchTrainingError = new List<double>();

            context.StartEpoch();
            foreach(var miniBatch in _provider.GetMiniBatches(context.BatchSize)) {
                var wireContext = new WireContext(context, miniBatch.Item2, _shouldBackpropagate);
                foreach (var target in _target)
                    target.Send(miniBatch.Item1, wireContext);
                miniBatchTrainingError.Add(wireContext.TrainingError);
                context.ApplyUpdates();
            }
            context.EndEpoch();

            double? trainingError = null;
            if(miniBatchTrainingError.Any())
                trainingError = miniBatchTrainingError.Average();

            return trainingError;
        }
    }
}
