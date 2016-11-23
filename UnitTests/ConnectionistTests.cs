using BrightWire;
using BrightWire.Connectionist;
using BrightWire.Connectionist.Training.Helper;
using BrightWire.Connectionist.Training.Layer.Recurrent;
using BrightWire.Helper;
using BrightWire.LinearAlgebra;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Helper;

namespace UnitTests
{
    [TestClass]
    public class ConnectionistTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = Provider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void XOR()
        {
            // create a template that describes each layer of the network
            // in this case no regularisation, and using a sigmoid activation function 
            var layerTemplate = new LayerDescriptor(0f) {
                Activation = ActivationType.Sigmoid
            };

            // Create some training data that the network will learn.  The XOR pattern looks like:
            // 0 0 => 0
            // 1 0 => 1
            // 0 1 => 1
            // 1 1 => 0
            var testDataProvider = _lap.NN.CreateTrainingDataProvider(XorData.Get());

            // create a batch trainer (hidden layer of size 4).
            using (var trainer = _lap.NN.CreateBatchTrainer(layerTemplate, testDataProvider.InputSize, 4, testDataProvider.OutputSize)) {
                // create a training context that will hold the training rate and batch size
                var trainingContext = _lap.NN.CreateTrainingContext(ErrorMetricType.OneHot, 0.03f, 2);

                // train the network!
                trainer.Train(testDataProvider, 1000, trainingContext);

                // execute the network to get the predictions
                var trainingResults = trainer.Execute(testDataProvider);
                for (var i = 0; i < trainingResults.Count; i++) {
                    var result = trainingResults[i];
                    var predictedResult = Convert.ToSingle(Math.Round(result.Output[0]));
                    var expectedResult = result.ExpectedOutput[0];
                    FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
                }

                // serialise the network parameters and data
                var networkData = trainer.NetworkInfo;

                // create a new network to execute the learned network
                var network = _lap.NN.CreateFeedForward(networkData);
                var results = XorData.Get().Select(d => Tuple.Create(network.Execute(d.Input), d.Output)).ToList();
                for (var i = 0; i < results.Count; i++) {
                    var result = results[i].Item1.AsIndexable();
                    var predictedResult = Convert.ToSingle(Math.Round(result[0]));
                    var expectedResult = results[i].Item2[0];
                    FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
                }
            }
        }

        [TestMethod]
        public void RecurrentAddition()
        {
            var trainingSet = BinaryIntegers.Addition(10, false).Select(l => l.ToArray()).ToList();

            const int HIDDEN_SIZE = 16, NUM_EPOCHS = 100, BATCH_SIZE = 32;
            var errorMetric = ErrorMetricType.BinaryClassification.Create();

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.LeakyRelu,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                DecayRate = 0.99f
            };

            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            var trainingDataProvider = _lap.NN.CreateSequentialTrainingDataProvider(trainingSet);
            var layers = new INeuralNetworkRecurrentLayer[] {
                _lap.NN.CreateSimpleRecurrentLayer(trainingDataProvider.InputSize, HIDDEN_SIZE, recurrentTemplate),
                _lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE, trainingDataProvider.OutputSize, layerTemplate)
            };
            RecurrentNetwork networkData = null;
            using (var trainer = _lap.NN.CreateRecurrentBatchTrainer(layers)) {
                var memory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                var trainingContext = _lap.NN.CreateTrainingContext(errorMetric, 0.1f, BATCH_SIZE);
                trainingContext.RecurrentEpochComplete += (tc, rtc) => {
                    Debug.WriteLine(tc.LastTrainingError);
                };
                trainer.Train(trainingDataProvider, memory, NUM_EPOCHS, _lap.NN.CreateRecurrentTrainingContext(trainingContext));
                networkData = trainer.NetworkInfo;
                networkData.Memory = new FloatArray {
                    Data = memory
                };
            }

            var network = _lap.NN.CreateRecurrent(networkData);
            foreach (var sequence in trainingSet) {
                var result = network.Execute(sequence.Select(d => d.Input).ToList());
            }
        }

        [TestMethod]
        public void LSTMAddition()
        {
            var trainingSet = BinaryIntegers.Addition(10, false).Select(l => l.ToArray()).ToList();

            const int HIDDEN_SIZE = 16, NUM_EPOCHS = 100, BATCH_SIZE = 32;
            var errorMetric = ErrorMetricType.BinaryClassification.Create();

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.LeakyRelu,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                DecayRate = 0.99f
            };

            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            var trainingDataProvider = _lap.NN.CreateSequentialTrainingDataProvider(trainingSet);
            var layers = new INeuralNetworkRecurrentLayer[] {
                    _lap.NN.CreateLstmRecurrentLayer(trainingDataProvider.InputSize, HIDDEN_SIZE, recurrentTemplate),
                    _lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE, trainingDataProvider.OutputSize, layerTemplate)
                };
            RecurrentNetwork networkData = null;
            using (var trainer = _lap.NN.CreateRecurrentBatchTrainer(layers)) {
                var memory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                var trainingContext = _lap.NN.CreateTrainingContext(errorMetric, 0.1f, BATCH_SIZE);
                trainingContext.RecurrentEpochComplete += (tc, rtc) => {
                    Debug.WriteLine(tc.LastTrainingError);
                };
                trainer.Train(trainingDataProvider, memory, NUM_EPOCHS, _lap.NN.CreateRecurrentTrainingContext(trainingContext));
                networkData = trainer.NetworkInfo;
                networkData.Memory = new FloatArray {
                    Data = memory
                };
            }

            var network = _lap.NN.CreateRecurrent(networkData);
            foreach (var sequence in trainingSet) {
                var result = network.Execute(sequence.Select(d => d.Input).ToList());
            }
        }

        [TestMethod]
        public void BidirectionalAddition()
        {
            var trainingSet = BinaryIntegers.Addition(10, false).Select(l => l.ToArray()).ToList();

            const int HIDDEN_SIZE = 16, NUM_EPOCHS = 100, BATCH_SIZE = 32;
            var errorMetric = ErrorMetricType.BinaryClassification.Create();

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.LeakyRelu,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                DecayRate = 0.99f
            };

            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            var trainingDataProvider = _lap.NN.CreateSequentialTrainingDataProvider(trainingSet);
            var layers = new INeuralNetworkBidirectionalLayer[] {
                    _lap.NN.CreateBidirectionalLayer(
                        _lap.NN.CreateSimpleRecurrentLayer(trainingDataProvider.InputSize, HIDDEN_SIZE, recurrentTemplate),
                        _lap.NN.CreateSimpleRecurrentLayer(trainingDataProvider.InputSize, HIDDEN_SIZE, recurrentTemplate)
                    ),
                    _lap.NN.CreateBidirectionalLayer(_lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE*2, trainingDataProvider.OutputSize, layerTemplate))
                };
            BidirectionalNetwork networkData = null;
            using (var trainer = _lap.NN.CreateBidirectionalBatchTrainer(layers)) {
                var forwardMemory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                var backwardMemory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                var trainingContext = _lap.NN.CreateTrainingContext(errorMetric, 0.1f, BATCH_SIZE);
                trainingContext.RecurrentEpochComplete += (tc, rtc) => {
                    Debug.WriteLine(tc.LastTrainingError);
                };
                trainer.Train(trainingDataProvider, forwardMemory, backwardMemory, NUM_EPOCHS, _lap.NN.CreateRecurrentTrainingContext(trainingContext));
                networkData = trainer.NetworkInfo;
                networkData.ForwardMemory = new FloatArray {
                    Data = forwardMemory
                };
                networkData.BackwardMemory = new FloatArray {
                    Data = backwardMemory
                };
            }

            var network = _lap.NN.CreateBidirectional(networkData);
            foreach (var sequence in trainingSet) {
                var result = network.Execute(sequence.Select(d => d.Input).ToList());
            }
        }
    }
}
