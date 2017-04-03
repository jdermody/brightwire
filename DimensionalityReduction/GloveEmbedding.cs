using BrightWire.Connectionist.Training.Layer;
using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Connectionist;
using BrightWire.Models;
using BrightWire.Connectionist.Training.WeightInitialisation;
using BrightWire.Connectionist.Training;
using System.Linq;
using BrightWire.Connectionist.Training.Helper;

namespace BrightWire.DimensionalityReduction
{
    public class GloveEmbedding
    {
        public struct SparseCooccurence
        {
            readonly ulong _data;

            //public uint Word1
            //{
            //    return _data
            //}
        }
        class WordVector : INeuralNetworkLayer
        {
            readonly IVector _bias;
            readonly IMatrix _weight;
            readonly int _vocabSize, _vectorSize;
            readonly INeuralNetworkLayerUpdater _layerUpdater;

            public WordVector(ILinearAlgebraProvider lap, int vocabSize, int vectorSize, IWeightInitialisation weightInit, float l2 = 0f)
            {
                _vocabSize = vocabSize;
                _vectorSize = vectorSize;
                _bias = lap.Create(vocabSize, x => weightInit.GetBias());
                _weight = lap.Create(vocabSize, vectorSize, (x, y) => weightInit.GetWeight(vocabSize, vectorSize, x, y));
                _layerUpdater = new UpdaterBase(this);
            }

            IActivationFunction INeuralNetworkLayer.Activation
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IVector Bias
            {
                get
                {
                    return _bias;
                }
            }

            public LayerDescriptor Descriptor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public int InputSize
            {
                get
                {
                    return _vocabSize;
                }
            }

            NetworkLayer INeuralNetworkLayer.LayerInfo
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public int OutputSize
            {
                get
                {
                    return _vectorSize;
                }
            }

            public IMatrix Weight
            {
                get
                {
                    return _weight;
                }
            }

            IMatrix INeuralNetworkLayer.Activate(IMatrix input)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                _weight.Dispose();
                _bias.Dispose();
            }

            IMatrix INeuralNetworkLayer.Execute(IMatrix input)
            {
                throw new NotImplementedException();
            }

            void INeuralNetworkLayer.Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, float learningRate)
            {
                if (biasDelta != null) {
                    using (var columnSums = biasDelta.ColumnSums())
                        _bias.SubtractInPlace(columnSums, 1f / columnSums.Count, learningRate);
                }
                _weight.SubtractInPlace(weightDelta, weightCoefficient, learningRate);
            }
            public INeuralNetworkLayerUpdater Updater { get { return _layerUpdater; } }
        }
        readonly ILinearAlgebraProvider _lap;
        readonly WordVector _word, _context;
        const float X_MAX = 100f, TRAINING_RATE = 0.0005f;

        public GloveEmbedding(ILinearAlgebraProvider lap, int vocabularySize, int vectorSize)
        {
            _lap = lap;
            var weightInit = new Gaussian(true);
            _word = new WordVector(lap, vocabularySize, vectorSize, weightInit);
            _context = new WordVector(lap, vocabularySize, vectorSize, weightInit);
        }

        public void Execute(int numIterations, IMatrix weight, IMatrix coocurrenceLog)
        {
            var trainingContext = new TrainingContext(TRAINING_RATE, 1, null);

            for (var i = 0; i < numIterations; i++) {
                var cost = _word.Weight.TransposeAndMultiply(_context.Weight);
                cost.AddToEachRow(_word.Bias);
                cost.AddToEachRow(_context.Bias);
                var costInner = cost.Subtract(coocurrenceLog);

                var costInnerSquared = costInner.Pow(2);
                //var cost3 = costInnerSquared.PointwiseMultiply(weight);
                var cost3 = weight.Multiply(costInnerSquared);
                var globalCost = cost3.AsIndexable().Values.Select(v => v / 2f).Sum();
                Console.WriteLine(globalCost);

                var weightCostInner = weight.Multiply(costInner);
                var gradMain = weightCostInner.Multiply(_context.Weight);
                var gradContext = weightCostInner.Multiply(_word.Weight);
                _word.Updater.Update(weightCostInner, gradMain, trainingContext);
                _context.Updater.Update(weightCostInner, gradContext, trainingContext);
            }
        }
    }
}
