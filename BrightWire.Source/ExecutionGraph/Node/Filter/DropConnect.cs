using BrightWire.ExecutionGraph.Node.Layer;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Filter
{
    /// <summary>
    /// Drop connect (inverted) regularisation
    /// http://cs.nyu.edu/~wanli/dropc/
    /// </summary>
    class DropConnect : FeedForward
    {
        new class Backpropagation : SingleBackpropagationBase<DropConnect>
        {
            readonly IMatrix _input, _filter, _filteredWeights;

            public Backpropagation(DropConnect source, IMatrix input, IMatrix filter, IMatrix filteredWeights) : base(source)
            {
                _input = input;
                _filter = filter;
                _filteredWeights = filteredWeights;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal against the filtered weights
                IMatrix ret = es.TransposeAndMultiply(_filteredWeights);

                // calculate the update to the weights and filter out the dropped connections
                var weightUpdate = _input.TransposeThisAndMultiply(es).PointwiseMultiply(_filter);

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }
        float _dropOutPercentage;
        Bernoulli _probabilityToDrop;

        public DropConnect(float dropOutPercentage, int inputSize, int outputSize, IVector bias, IMatrix weight, bool stochastic, IGradientDescentOptimisation updater, string name = null) 
            : base(inputSize, outputSize, bias, weight, updater, name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = stochastic ? new Bernoulli(_dropOutPercentage) : new Bernoulli(_dropOutPercentage, new System.Random(0));
        }

        public override void ExecuteForward(IContext context)
        {
            if (context.IsTraining) {
                var lap = context.LinearAlgebraProvider;
                var input = context.Data;
                var inputMatrix = input.GetMatrix();
                var filter = lap.CreateMatrix(Weight.RowCount, Weight.ColumnCount, (i, j) => _probabilityToDrop.Sample() == 1 ? 0f : 1f / _dropOutPercentage);
                var filteredWeights = Weight.PointwiseMultiply(filter);
                var output = _FeedForward(inputMatrix, filteredWeights);
                _AddNextGraphAction(context, input.ReplaceWith(output), () => new Backpropagation(this, inputMatrix, filter, filteredWeights));
            } else
                base.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("DC", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);
            _dropOutPercentage = reader.ReadSingle();
            if(_probabilityToDrop == null)
                _probabilityToDrop = new Bernoulli(_dropOutPercentage);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_dropOutPercentage);
        }
    }
}
