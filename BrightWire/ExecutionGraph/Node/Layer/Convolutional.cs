using BrightWire.ExecutionGraph.Helper;
using System;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Layer
{
	/// <summary>
	/// Convolutional neural network layer
	/// https://en.wikipedia.org/wiki/Convolutional_neural_network
	/// </summary>
    internal class Convolutional : NodeBase
	{
		class Backpropagation(Convolutional source, ITensor3D im2Col, uint inputWidth, uint inputHeight, uint depth, uint newWidth, uint newHeight)
            : SingleBackpropagationBase<Convolutional>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var tensor = errorSignal.GetMatrix().Reshape(null, _source._filter.ColumnCount, newHeight, newWidth);
                var padding = _source._padding;

                // calculate the weight and bias updates
                using (var update = im2Col.TransposeThisAndMultiply(tensor)) {
                    var weightUpdate = update.AddAllMatrices();
                    var biasUpdate = tensor.ColumnSums();

                    var learningContext = context.LearningContext!;
                    learningContext.AddError(NodeErrorType.Weight, _source, weightUpdate);
                    learningContext.AddError(NodeErrorType.Bias, _source, biasUpdate);
                }

                if (_source._shouldBackpropagate) {
                    var filters = _source._filter;
                    var inputDepth = _source._inputDepth;
                    var filterWidth = _source._filterWidth;
                    var filterHeight = _source._filterHeight;
                    var xStride = _source._xStride;
                    var yStride = _source._yStride;

                    var outputRows = inputHeight + padding * 2;
                    var outputColumns = inputWidth + padding * 2;
                    var outputDepth = depth;

                    var reverseIm2Col = tensor.ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
                    var delta = reverseIm2Col;
                    if (padding > 0) {
                        var delta2 = delta.RemovePadding(padding);
                        delta.Dispose();
                        delta = delta2;
                    }

                    var deltaMatrix = delta.ReshapeAsMatrix();
                    var ret = new Tensor4DGraphData(deltaMatrix, inputHeight, inputWidth, inputDepth);
                    return ret;
                }
                return GraphData.Null;
            }
        }
		IGradientDescentOptimisation? _updater;
		uint _padding, _filterWidth, _filterHeight, _xStride, _yStride, _inputDepth;
		IMatrix _filter;
		IVector _bias;
		bool _shouldBackpropagate;

		public Convolutional(
			bool shouldBackpropagate,
			IWeightInitialisation weightInitialisation,
			Func<IMatrix, IGradientDescentOptimisation> updater,
			uint inputDepth,
			uint filterCount,
			uint padding,
			uint filterWidth,
			uint filterHeight,
			uint xStride, 
			uint yStride,
			string? name = null) : base(name)
		{
			_shouldBackpropagate = shouldBackpropagate;
			_padding = padding;
			_filterWidth = filterWidth;
			_filterHeight = filterHeight;
			_xStride = xStride;
			_yStride = yStride;
			_inputDepth = inputDepth;

			_bias = weightInitialisation.CreateBias(filterCount);
			_filter = weightInitialisation.CreateWeight(_filterWidth * _filterHeight * _inputDepth, filterCount);
			_updater = updater(_filter);
		}

		public uint FilterCount => _filter.ColumnCount;

		protected override void DisposeInternal(bool isDisposing)
		{
			_filter.Dispose();
			_bias.Dispose();
		}

        public override void ApplyError(NodeErrorType type, ITensor delta, ILearningContext context)
        {
            if(type == NodeErrorType.Bias)
                _bias.AddInPlace(delta, 1f, context.LearningRate);
			else if (type == NodeErrorType.Weight)
                _updater!.Update(_filter, (IMatrix)delta, context);
            else
                throw new NotImplementedException();
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var tensor = signal.GetMatrix().Reshape(null, signal.Depth, signal.Rows, signal.Columns);

            var inputWidth = tensor.ColumnCount;
            var inputHeight = tensor.RowCount;
            var newWidth = ((inputWidth - _filterWidth + (2 * _padding)) / _xStride) + 1;
            var newHeight = ((inputHeight - _filterHeight + (2 * _padding)) / _yStride) + 1;

            if (_padding > 0)
                tensor = tensor.AddPadding(_padding);

            var im2Col = tensor.Im2Col(_filterWidth, _filterHeight, _xStride, _yStride);
            var outputSignal = im2Col.MultiplyEachMatrixBy(_filter);
            outputSignal.AddToEachRow(_bias);
            var outputTensor = outputSignal.Reshape(tensor.Count, FilterCount, newHeight, newWidth);

            var graphData = new Tensor4DGraphData(outputTensor);
            return (this, graphData, () => new Backpropagation(this, im2Col, inputWidth, inputHeight, tensor.Depth, newWidth, newHeight));
        }

        protected override (string Description, byte[] Data) GetInfo()
		{
			return ("CON", WriteData(WriteTo));
		}

		public override void ReadFrom(GraphFactory factory, BinaryReader reader)
		{
			var lap = factory.LinearAlgebraProvider;

			_padding = (uint)reader.ReadInt32();
			_filterWidth = (uint)reader.ReadInt32();
			_filterHeight = (uint)reader.ReadInt32();
			_xStride = (uint)reader.ReadInt32();
			_yStride = (uint)reader.ReadInt32();
			_inputDepth = (uint)reader.ReadInt32();
			_shouldBackpropagate = reader.ReadBoolean();

			// read the bias parameters
			var bias = factory.Context.LoadReadOnlyVectorFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_bias == null)
                _bias = bias.Create(lap);
            else
                bias.CopyTo(_bias);

            // read the weight parameters
			var weight = factory.Context.ReadMatrixFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_filter == null)
				_filter = weight.Create(lap);
            else
                weight.CopyTo(_filter);

            // read the updater
			_updater ??= factory.CreateWeightUpdater(_filter);
		}

		public override void WriteTo(BinaryWriter writer)
		{
			writer.Write(_padding);
			writer.Write(_filterWidth);
			writer.Write(_filterHeight);
			writer.Write(_xStride);
			writer.Write(_yStride);
			writer.Write(_inputDepth);
			writer.Write(_shouldBackpropagate);

			_bias.WriteTo(writer);
			_filter.WriteTo(writer);
		}
	}
}
