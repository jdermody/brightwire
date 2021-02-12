using BrightWire.ExecutionGraph.Helper;
using System;
using System.Diagnostics;
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
		class Backpropagation : SingleBackpropagationBase<Convolutional>
		{
			readonly I3DFloatTensor _im2Col;
			readonly uint _inputWidth, _inputHeight, _inputDepth, _newWidth, _newHeight;

			public Backpropagation(Convolutional source, I3DFloatTensor im2Col, uint inputWidth, uint inputHeight, uint inputDepth, uint newWidth, uint newHeight)
				: base(source)
			{
				_im2Col = im2Col;
				_newWidth = newWidth;
				_newHeight = newHeight;
				_inputWidth = inputWidth;
				_inputHeight = inputHeight;
				_inputDepth = inputDepth;
			}

			void UpdateBias(IFloatVector delta, ILearningContext context)
			{
				_source._bias.AddInPlace(delta, 1f, context.BatchLearningRate);
			}

			protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
			{
				var tensor = errorSignal.GetMatrix().ReshapeAs4DTensor(_newHeight, _newWidth, _source._filter.ColumnCount);
				var padding = _source._padding;

				// calculate the weight and bias updates
				using (var update = _im2Col.TransposeThisAndMultiply(tensor)) {
					var weightUpdate = update.CombineDepthSlices();
					var biasUpdate = tensor.ColumnSums();

					// TODO: divide bias update by number of columns?

                    var learningContext = context.LearningContext!;
                    learningContext.StoreUpdate(_source, weightUpdate, err => _source.Update(err, learningContext));
                    learningContext.StoreUpdate(_source, biasUpdate, bu => UpdateBias(bu, learningContext));
				}

				if (_source._shouldBackpropagate) {
					var filters = _source._filter;
					var inputDepth = _source._inputDepth;
					var filterWidth = _source._filterWidth;
					var filterHeight = _source._filterHeight;
					var xStride = _source._xStride;
					var yStride = _source._yStride;

					var outputRows = _inputHeight + padding * 2;
					var outputColumns = _inputWidth + padding * 2;
					var outputDepth = _inputDepth;

					var reverseIm2Col = tensor.ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
					var delta = reverseIm2Col;
					if (padding > 0) {
						var delta2 = delta.RemovePadding(padding);
						delta.Dispose();
						delta = delta2;
					}
					return new Tensor4DGraphData(delta.ReshapeAsMatrix(), _inputHeight, _inputWidth, inputDepth);
				}
				return NullGraphData.Instance;
			}

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var tensor = errorSignal.GetMatrix().ReshapeAs4DTensor(_newHeight, _newWidth, _source._filter.ColumnCount);
                var padding = _source._padding;

                // calculate the weight and bias updates
                using (var update = _im2Col.TransposeThisAndMultiply(tensor)) {
                    var weightUpdate = update.CombineDepthSlices();
                    var biasUpdate = tensor.ColumnSums();

                    var learningContext = context.LearningContext!;
                    learningContext.StoreUpdate(_source, weightUpdate, err => _source.Update(err, learningContext));
                    learningContext.StoreUpdate(_source, biasUpdate, bu => UpdateBias(bu, learningContext));
                }

                if (_source._shouldBackpropagate) {
                    var filters = _source._filter;
                    var inputDepth = _source._inputDepth;
                    var filterWidth = _source._filterWidth;
                    var filterHeight = _source._filterHeight;
                    var xStride = _source._xStride;
                    var yStride = _source._yStride;

                    var outputRows = _inputHeight + padding * 2;
                    var outputColumns = _inputWidth + padding * 2;
                    var outputDepth = _inputDepth;

                    var reverseIm2Col = tensor.ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
                    var delta = reverseIm2Col;
                    if (padding > 0) {
                        var delta2 = delta.RemovePadding(padding);
                        delta.Dispose();
                        delta = delta2;
                    }
                    return new Tensor4DGraphData(delta.ReshapeAsMatrix(), _inputHeight, _inputWidth, inputDepth);
                }
                return NullGraphData.Instance;
            }
        }
		IGradientDescentOptimisation? _updater;
		uint _padding, _filterWidth, _filterHeight, _xStride, _yStride, _inputDepth;
		IFloatMatrix _filter;
		IFloatVector _bias;
		bool _shouldBackpropagate;

		public Convolutional(
			bool shouldBackpropagate,
			IWeightInitialisation weightInitialisation,
			Func<IFloatMatrix, IGradientDescentOptimisation> updater,
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

		public void Update(IFloatMatrix delta, ILearningContext context)
		{
			_updater!.Update(_filter, delta, context);
		}

		public override void ExecuteForward(IGraphSequenceContext context)
		{
			var input = context.Data;
			var tensor = input.GetMatrix().ReshapeAs4DTensor(input.Rows, input.Columns, input.Depth);

			var inputWidth = tensor.ColumnCount;
			var inputHeight = tensor.RowCount;
			var newWidth = ((inputWidth - _filterWidth + (2 * _padding)) / _xStride) + 1;
			var newHeight = ((inputHeight - _filterHeight + (2 * _padding)) / _yStride) + 1;

			if (_padding > 0)
				tensor = tensor.AddPadding(_padding);

			var im2Col = tensor.Im2Col(_filterWidth, _filterHeight, _xStride, _yStride);
			var outputSignal = im2Col.Multiply(_filter);
			outputSignal.AddToEachRow(_bias);
			var outputTensor = outputSignal.ReshapeAs4DTensor(newHeight, newWidth);
			Debug.Assert(outputTensor.Depth == FilterCount && outputTensor.Count == tensor.Count);

			var graphData = new Tensor4DGraphData(outputTensor);
			AddNextGraphAction(context, graphData, () => new Backpropagation(this, im2Col, inputWidth, inputHeight, tensor.Depth, newWidth, newHeight));
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
			var bias = factory.Context.ReadVectorFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_bias == null)
				_bias = lap.CreateVector(bias);
			else
				_bias.Data = bias;

			// read the weight parameters
			var weight = factory.Context.ReadMatrixFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_filter == null)
				_filter = lap.CreateMatrix(weight);
			else
				_filter.Data = weight;

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

			_bias.Data.WriteTo(writer);
			_filter.Data.WriteTo(writer);
		}
	}
}
