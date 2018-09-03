using BrightWire.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Batch normalisation layer
    /// https://arxiv.org/abs/1502.03167
    /// </summary>
    class BatchNormalisation : NodeBase
    {
        // backward implementation from https://github.com/kevinzakka/research-paper-notes/blob/master/batch_norm.py
        class Backpropagation : SingleBackpropagationBase<BatchNormalisation>
        {
            readonly IMatrix _inputMinusMean, _inverseVariance, _xHat, _gamma;

            public Backpropagation(BatchNormalisation source, IMatrix inputMinusMean, IMatrix inverseVariance, IMatrix xHat, IMatrix gamma)
                : base(source)
            {
                _inputMinusMean = inputMinusMean;
                _inverseVariance = inverseVariance;
                _xHat = xHat;
                _gamma = gamma;
            }
            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var error = errorSignal.GetMatrix();
                var batchSize = (float)context.BatchSequence.MiniBatch.BatchSize;
                var lap = context.LinearAlgebraProvider;

                using (var dxHat = error.PointwiseMultiply(_gamma))
                using (var temp = dxHat.PointwiseMultiply(_inputMinusMean))
                using (var temp2 = _inverseVariance.PointwiseMultiply(_inverseVariance))
                using (var inverseVarianceCubed = temp2.PointwiseMultiply(_inverseVariance))
                using (var temp3 = temp.PointwiseMultiply(inverseVarianceCubed)) {
                    temp3.Multiply(-0.5f);

                    using (var dVar = temp3.ColumnSums())
                    using (var temp4 = dxHat.PointwiseMultiply(_inverseVariance)) {
                        temp4.Multiply(-1f);

                        using (var dmu = temp4.ColumnSums())
                        using (var temp5 = _inputMinusMean.ColumnSums()) {
                            temp5.Multiply(-2f / batchSize);

                            using (var dmu2 = temp5.PointwiseMultiply(dVar)) {
                                dmu.AddInPlace(dmu2);

                                using (var dVarMatrix = lap.CreateMatrixFromRows(Enumerable.Repeat(dVar, context.BatchSequence.MiniBatch.BatchSize).ToList()))
                                using (var dx1 = dxHat.PointwiseMultiply(_inverseVariance))
                                using (var dx2 = dVarMatrix.PointwiseMultiply(_inputMinusMean)) {
                                    dx2.Multiply(-2f / batchSize);

                                    using (var dx3 = lap.CreateMatrixFromRows(Enumerable.Repeat(dmu, context.BatchSequence.MiniBatch.BatchSize).ToList())) {
                                        dx3.Multiply(1f / batchSize);

                                        var dx = dx1.Add(dx2);
                                        dx.AddInPlace(dx3);

                                        var dBeta = dx.ColumnSums();
                                        using (var temp6 = _xHat.PointwiseMultiply(error)) {
                                            var dGamma = temp6.ColumnSums();

                                            // store the updates
                                            var learningContext = context.LearningContext;
                                            learningContext.StoreUpdate(_source, dBeta, err => _source._beta.AddInPlace(err, 1f, learningContext.BatchLearningRate));
                                            learningContext.StoreUpdate(_source, dGamma, err => _source._gamma.AddInPlace(err, 1f, learningContext.BatchLearningRate));
                                        }

                                        return errorSignal.ReplaceWith(dx);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        int _inputSize;
        float _momentum;
        IVector _gamma, _beta, _mean, _variance;

        public BatchNormalisation(GraphFactory graph, int inputSize, IWeightInitialisation weightInit, float momentum = 0.9f, string name = null) : base(name)
        {
            _inputSize = inputSize;
            _momentum = momentum;
            var lap = graph.LinearAlgebraProvider;

            using (var matrix = weightInit.CreateWeight(1, _inputSize)) {
                _Create(graph,
                    gamma: lap.CreateVector(Enumerable.Repeat(1f, _inputSize)),
                    beta: lap.CreateVector(Enumerable.Repeat(0f, _inputSize)),
                    mean: lap.CreateVector(Enumerable.Repeat(0f, _inputSize)),
                    variance: lap.CreateVector(Enumerable.Repeat(1f, _inputSize))
                );
            }
        }

        void _Create(GraphFactory graph, IVector gamma, IVector beta, IVector mean, IVector variance)
        {
            _gamma = gamma;
            _beta = beta;
            _mean = mean;
            _variance = variance;
        }

        I4DTensor _FormIntoTensor(ILinearAlgebraProvider lap, IVector vector, I4DTensor tensor)
        {
            var indexableVector = vector.AsIndexable();
            var tensorList = new List<I3DTensor>();

            for(var i = 0; i < tensor.Count; i++) {
                var matrixList = new List<IMatrix>();
                for(var j = 0; j < tensor.Depth; j++)
                    matrixList.Add(lap.CreateMatrix(tensor.RowCount, tensor.ColumnCount, indexableVector[j]));
                tensorList.Add(lap.Create3DTensor(matrixList));
            }
            return lap.Create4DTensor(tensorList);
        }

        public override void ExecuteForward(IContext context)
        {
            var lap = context.LinearAlgebraProvider;
            var input = context.Data.GetMatrix();
            IMatrix output, inputMinusMean = null, inverseVariance = null, xHat = null, gamma = null;

            Debug.Assert(input.RowCount == context.BatchSequence.MiniBatch.BatchSize);
            var batchSize = input.RowCount;

            var tensor = context.Data.Get4DTensor();
            if (tensor != null) {
                IVector currentVariance, currentMean;
                if (context.IsTraining) {
                    var volumeList = input.AsVector().Split(tensor.Count);
                    var rowList = volumeList.Select(m => lap.CreateVector(m.Split(tensor.Depth).Select(v => v.Average()))).ToList();
                    var reducedInput = lap.CreateMatrixFromRows(rowList);

                    // calculate batch mean and variance
                    currentMean = reducedInput.ColumnSums();
                    currentMean.Multiply(1f / reducedInput.RowCount);
                    using (var meanMatrix = lap.CreateMatrixFromRows(Enumerable.Repeat(currentMean, reducedInput.RowCount).ToList())) 
                    using (var reducedInputMinusMean = reducedInput.Subtract(meanMatrix))
                    using (var temp = reducedInputMinusMean.PointwiseMultiply(reducedInputMinusMean)) {
                        currentVariance = temp.ColumnSums();
                        currentVariance.Multiply(1f / reducedInput.RowCount);
                    }
                } else {
                    currentVariance = _variance;
                    currentMean = _mean;
                }
                using (var tensorMean = _FormIntoTensor(lap, currentMean, tensor))
                using (var tensorVariance = _FormIntoTensor(lap, currentVariance, tensor))
                using (var tensorGamma = _FormIntoTensor(lap, _gamma, tensor))
                using (var tensorBeta = _FormIntoTensor(lap, _beta, tensor))
                using (var matrixMean = tensorMean.AsMatrix())
                using (var matrixVariance = tensorVariance.AsMatrix())
                using (var matrixBeta = tensorBeta.AsMatrix())
                using (var temp = input.Subtract(matrixMean))
                using (var sqrtVariance = matrixVariance.Sqrt(1e-6f)) {
                    gamma = tensorMean.AsMatrix();
                    inputMinusMean = input.Subtract(matrixMean);

                    // calculate batch normalisation
                    using (var ones = context.LinearAlgebraProvider.CreateMatrix(batchSize, input.ColumnCount, 1f)) {
                        inverseVariance = ones.PointwiseDivide(sqrtVariance);
                        xHat = inputMinusMean.PointwiseMultiply(inverseVariance);
                        output = xHat.PointwiseMultiply(gamma);
                        output.AddToEachRow(_beta);

                        // update the mean
                        _mean.AddInPlace(currentMean, _momentum, 1f - _momentum);

                        // correct for the biased sample variance
                        currentVariance.Multiply(1f / (batchSize - 1));

                        // update the variance
                        _variance.AddInPlace(currentVariance, _momentum, 1f - _momentum);

                        if (context.IsTraining) {
                            currentVariance.Dispose();
                            currentMean.Dispose();
                        }
                    }
                }
            } else {
                if (context.IsTraining) {
                    var learningContext = context.LearningContext;
                    gamma = lap.CreateMatrixFromRows(Enumerable.Repeat(_gamma, batchSize).ToList());

                    // calculate batch mean
                    var batchMean = input.ColumnSums();
                    batchMean.Multiply(1f / batchSize);

                    // find input minus mean
                    using (var meanMatrix = lap.CreateMatrixFromRows(Enumerable.Repeat(batchMean, batchSize).ToList()))
                        inputMinusMean = input.Subtract(meanMatrix);

                    // calculate variance as (x - u)^2
                    IMatrix batchVariance = inputMinusMean.PointwiseMultiply(inputMinusMean);

                    // calculate batch normalisation
                    using (var varianceSqrt = batchVariance.Sqrt(1e-6f))
                    using (var ones = context.LinearAlgebraProvider.CreateMatrix(batchSize, input.ColumnCount, 1f)) {
                        inverseVariance = ones.PointwiseDivide(varianceSqrt);
                        xHat = inputMinusMean.PointwiseMultiply(inverseVariance);
                        output = xHat.PointwiseMultiply(gamma);
                        output.AddToEachRow(_beta);

                        // update the mean
                        _mean.AddInPlace(batchMean, 1f, learningContext.BatchLearningRate);

                        // correct for the biased sample variance
                        batchVariance.Multiply(1f / (batchSize - 1));

                        // update the variance
                        using(var temp = batchVariance.ColumnSums()) {
                            temp.Multiply(1f / batchSize);
                            _variance.AddInPlace(temp, _momentum, learningContext.BatchLearningRate);
                        }
                        batchVariance.Dispose();
                    }
                } else {
                    using (var varianceMatrix = lap.CreateMatrixFromRows(Enumerable.Repeat(_variance, batchSize).ToList()))
                    using (var varianceMatrixSqrt = varianceMatrix.Sqrt(1e-6f))
                    using (var meanMatrix = lap.CreateMatrixFromRows(Enumerable.Repeat(_mean, batchSize).ToList()))
                    using (var gammaMatrix = lap.CreateMatrixFromRows(Enumerable.Repeat(_gamma, batchSize).ToList()))
                    using (var inputSubtractMean = input.Subtract(meanMatrix))
                    using (var temp = inputSubtractMean.PointwiseMultiply(gammaMatrix)) {
                        output = temp.PointwiseDivide(varianceMatrixSqrt);
                        output.AddToEachRow(_beta);
                    }
                }
                _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, inputMinusMean, inverseVariance, xHat, gamma));
            }
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("BN", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _inputSize = reader.ReadInt32();
            _momentum = reader.ReadSingle();
            var gamma = FloatVector.ReadFrom(reader);
            var beta = FloatVector.ReadFrom(reader);
            var mean = FloatVector.ReadFrom(reader);
            var variance = FloatVector.ReadFrom(reader);

            if (_gamma == null) {
                var lap = factory.LinearAlgebraProvider;
                _Create(factory, lap.CreateVector(gamma), lap.CreateVector(beta), lap.CreateVector(mean), lap.CreateVector(variance));
            } else {
                _gamma.Data = gamma;
                _beta.Data = beta;
                _mean.Data = mean;
                _variance.Data = variance;
            }
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_inputSize);
            writer.Write(_momentum);
            _gamma.Data.WriteTo(writer);
            _beta.Data.WriteTo(writer);
            _mean.Data.WriteTo(writer);
            _variance.Data.WriteTo(writer);
        }
    }
}
