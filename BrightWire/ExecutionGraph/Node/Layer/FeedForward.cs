using System;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Feed forward neural network
    /// https://en.wikipedia.org/wiki/Feedforward_neural_network
    /// </summary>
    internal class FeedForward(uint inputSize, uint outputSize, IVector<float> bias, IMatrix<float> weight, IGradientDescentOptimisation updater, string? name = null)
        : NodeBase(name), IFeedForward
    {
        IGradientDescentOptimisation _updater = updater;

        protected class Backpropagation(FeedForward source, IMatrix<float> input) : SingleBackpropagationBase<FeedForward>(source)
        {
            protected override void DisposeMemory(bool isDisposing)
            {
                //_input.Dispose();
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                var ret = es.TransposeAndMultiply(_source.Weight);

                // calculate the update to the weights
                var weightUpdate = input.TransposeThisAndMultiply(es);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.AddError(NodeErrorType.Bias, _source, es);
                learningContext.AddError(NodeErrorType.Weight, _source, weightUpdate);

                return errorSignal.ReplaceWith(ret);
            }
        }

        public IVector<float> Bias { get; private set; } = bias;
        public IMatrix<float> Weight { get; private set; } = weight;
        public uint InputSize { get; private set; } = inputSize;
        public uint OutputSize { get; private set; } = outputSize;

        protected override void DisposeInternal(bool isDisposing)
        {
            Bias.Dispose();
            Weight.Dispose();
        }

        public override void ApplyError(NodeErrorType type, ITensor<float> delta, ILearningContext context)
        {
            if(type == NodeErrorType.Bias)
                UpdateBias((IMatrix<float>)delta, context);
            else if (type == NodeErrorType.Weight)
                UpdateWeights((IMatrix<float>)delta, context);
            else
                throw new NotImplementedException();
        }

        public void UpdateWeights(IMatrix<float> delta, ILearningContext context)
        {
            _updater.Update(Weight, delta, context);
            //if(!Weight.IsEntirelyFinite())
            //    Debugger.Break();
        }

        public void UpdateBias(IMatrix<float> delta, ILearningContext context)
        {
            using var columnSums = delta.ColumnSums();
            columnSums.MultiplyInPlace(1f / delta.RowCount);
            Bias.AddInPlace(columnSums, 1f, context.LearningRate);
        }

        protected IMatrix<float> FeedForwardInternal(IMatrix<float> input, IMatrix<float> weight)
        {
            var output = input.Multiply(weight);
            output.AddToEachRow(Bias.Segment);
            return output;
        }

        public IMatrix<float> Forward(IMatrix<float> input) => FeedForwardInternal(input, Weight);

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = FeedForwardInternal(input, Weight);
            //if(!output.IsEntirelyFinite())
            //    Debugger.Break();

            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("FF", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory.LinearAlgebraProvider;
            InputSize = (uint)reader.ReadInt32();
            OutputSize = (uint)reader.ReadInt32();

            // read the bias parameters
            var bias = factory.Context.LoadReadOnlyVectorFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Bias == null)
                Bias = bias.Create(lap);
            else
                bias.CopyTo(Bias);

            // read the weight parameters
            var weight = factory.Context.ReadMatrixFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Weight == null)
                Weight = weight.Create(lap);
            else
                weight.CopyTo(Weight);

            // ReSharper disable once ConstantNullCoalescingCondition
            _updater ??= factory.CreateWeightUpdater(Weight);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)InputSize);
            writer.Write((int)OutputSize);
            Bias.WriteTo(writer);
            Weight.WriteTo(writer);
        }
    }
}
