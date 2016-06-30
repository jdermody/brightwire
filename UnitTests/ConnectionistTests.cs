using BrightWire;
using BrightWire.Connectionist;
using BrightWire.Helper;
using BrightWire.LinearAlgebra;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            _lap = new NumericsProvider();
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

            // create a (CPU based) linear algebra provider
            using (var lap = new NumericsProvider()) {
                FeedForwardNetwork networkData;
                // Create some training data that the network will learn.  The XOR pattern looks like:
                // 0 0 => 0
                // 1 0 => 1
                // 0 1 => 1
                // 1 1 => 0
                var testDataProvider = new DenseTrainingDataProvider(_lap, XorData.Get());

                // create a batch trainer.  This network has a hidden layer of size 4,
                // and input and outputs of 2 and 1 respectively.
                using (var trainer = _lap.NN.CreateBatchTrainer(layerTemplate, 2, 4, 1)) {
                    // create a training context that will hold the training rate and batch size
                    var trainingContext = _lap.NN.CreateTrainingContext(0.03f, 2);

                    // train the network!
                    trainer.Train(testDataProvider, 1000, trainingContext);

                    // execute the network to get the predictions
                    var trainingResults = trainer.Execute(testDataProvider);
                    for (var i = 0; i < trainingResults.Count; i++) {
                        var result = trainingResults[i];
                        var predictedResult = Convert.ToSingle(Math.Round(result.Item1[0]));
                        var expectedResult = result.Item2[0];
                        FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
                    }

                    // serialise the network parameters and data
                    networkData = trainer.NetworkInfo;
                }

                // create a new network to execute the learned network
                var network = lap.NN.CreateFeedForward(networkData);
                var results = XorData.Get().Select(d => Tuple.Create(network.Execute(d.Item1), d.Item2)).ToList();
                for (var i = 0; i < results.Count; i++) {
                    var result = results[i].Item1.AsIndexable();
                    var predictedResult = Convert.ToSingle(Math.Round(result[0]));
                    var expectedResult = results[i].Item2[0];
                    FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
                }
            }
        }
    }
}
