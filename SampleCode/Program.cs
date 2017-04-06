using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.DimensionalityReduction;
using BrightWire.Ensemble;
using BrightWire.Helper;
using BrightWire.Models.Input;
using BrightWire.Models.Output;
using BrightWire.TrainingData;
using BrightWire.TrainingData.Artificial;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        public static void XOR()
        {
            var lap = Provider.CreateLinearAlgebra();

            // create a template that describes each layer of the network
            // in this case no regularisation, and using a sigmoid activation function 
            var layerTemplate = new LayerDescriptor(0f) {
                Activation = ActivationType.Sigmoid,
                LayerTrainer = LayerTrainerType.Standard,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                WeightUpdate = WeightUpdateType.RMSprop
            };

            // Create some training data that the network will learn.  The XOR pattern looks like:
            // 0 0 => 0
            // 1 0 => 1
            // 0 1 => 1
            // 1 1 => 0
            var testDataProvider = lap.NN.CreateTrainingDataProvider(XorData.Get());

            // create a batch trainer (hidden layer of size 4).
            using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, testDataProvider.InputSize, 4, testDataProvider.OutputSize)) {
                // create a training context that will hold the training rate and batch size
                var trainingContext = lap.NN.CreateTrainingContext(ErrorMetricType.OneHot, 0.03f, 2);

                trainingContext.EpochComplete +=
                    ctx => Console.WriteLine($"{ctx.CurrentEpoch}) {ctx.LastTrainingError}");

                // train the network!
                trainer.Train(testDataProvider, 1000, trainingContext);

                // execute the network to get the predictions
                var trainingResults = trainer.Execute(testDataProvider);
                for (var i = 0; i < trainingResults.Count; i++) {
                    var result = trainingResults[i];
                    var predictedResult = Convert.ToSingle(Math.Round(result.Output[0]));
                    var expectedResult = result.ExpectedOutput[0];
                }

                // serialise the network parameters and data
                var networkData = trainer.NetworkInfo;

                // create a new network to execute the learned network
                var network = lap.NN.CreateFeedForward(networkData);
                var results = XorData.Get().Select(d => Tuple.Create(network.Execute(d.Input), d.Output)).ToList();
                for (var i = 0; i < results.Count; i++) {
                    var result = results[i].Item1.AsIndexable();
                    var predictedResult = Convert.ToSingle(Math.Round(result[0]));
                    var expectedResult = results[i].Item2[0];
                }
            }
        }

        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            //XOR();
            //ReducedMNIST(@"D:\data\mnist\");
            //MNISTConvolutional(@"D:\data\mnist\");

            //IrisClassification();
            //IrisClustering();
            //MarkovChains();
            //MNIST(@"D:\data\mnist\", @"D:\data\mnist\model_test.dat");
            //SentimentClassification(@"D:\data\sentiment labelled sentences\");
            //TextClustering(@"D:\data\[UCI] AAAI-14 Accepted Papers - Papers.csv", @"d:\temp\");
            IntegerAddition();
            //IncomePrediction(@"d:\data\adult.data", @"d:\data\adult.test");
            //SequenceClassification();
            //SequenceClassification3();
            //SequenceClassification3();
            //SequenceToClassification(@"D:\data\sentiment labelled sentences\", @"d:\temp\sentiment.dat");
            //TestSequenceClassification(@"d:\temp\sentiment.dat");
            //SequenceToSequence();
        }
    }
}
