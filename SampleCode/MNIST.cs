using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.Helper;
using BrightWire.Models.Convolutional;
using BrightWire.TrainingData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        /// <summary>
        /// Trains a neural net on the MNIST database (digit recognition)
        /// The data files can be downloaded from http://yann.lecun.com/exdb/mnist/
        /// </summary>
        /// <param name="dataFilesPath">The path to a directory with the four extracted data files</param>
        public static void MNIST(string dataFilesPath, string outputModelPath)
        {
            // neural network hyper parameters
            const int HIDDEN_SIZE = 1024, BATCH_SIZE = 128, NUM_EPOCHS = 40;
            const float TRAINING_RATE = 0.03f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0f) {
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };

            Console.Write("Loading training data...");
            var trainingData = Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte");
            var testData = Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte");
            Console.WriteLine("done");

            Console.WriteLine("Starting training...");
            using (var lap = GPUProvider.CreateLinearAlgebra()) {
                var trainingSet = lap.NN.CreateTrainingDataProvider(trainingData.Select(d => d.Sample).ToList());
                var testSet = lap.NN.CreateTrainingDataProvider(testData.Select(d => d.Sample).ToList());

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, Mnist.INPUT_SIZE, HIDDEN_SIZE, Mnist.OUTPUT_SIZE)) {
                    var trainingManager = lap.NN.CreateFeedForwardManager(trainer, outputModelPath, testSet);
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.ScheduleTrainingRateChange(NUM_EPOCHS / 2, TRAINING_RATE / 3);
                    trainingManager.Train(trainingSet, NUM_EPOCHS, trainingContext);
                }
            }
        }

        public static void MNISTConvolutional(string dataFilesPath)
        {
            Console.Write("Loading training data...");
            var trainingData = Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte");
            var testData = Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte");
            Console.WriteLine($"done - found {trainingData.Count} training samples and {testData.Count} test samples");

            var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                Stride = 1,
                Padding = 1,
                FilterDepth = 6,
                FilterHeight = 3,
                FilterWidth = 3,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            const int HIDDEN_SIZE = 128, BATCH_SIZE = 128, NUM_EPOCHS = 200;
            const float TRAINING_RATE = 0.03f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0.1f) {
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var trainingSamples = trainingData.Shuffle(0).Take(6000).Select(d => d.Convolutional).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var testSamples = testData.Shuffle(0).Take(1000).Select(d => d.Convolutional).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var layer = new[] {
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.FilterSize, 28),
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.FilterSize * convolutionDescriptor.FilterDepth, 28)
                };
                var trainingDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, trainingSamples, layer, true);
                var testDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, testSamples, layer, false);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingDataProvider.InputSize, HIDDEN_SIZE, trainingDataProvider.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.EpochComplete += c => {
                        var output = trainer.Execute(testDataProvider);
                        var testError = output.Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                        trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                    };
                    trainingContext.ScheduleTrainingRateChange(50, TRAINING_RATE / 3f);
                    trainingContext.ScheduleTrainingRateChange(100, TRAINING_RATE / 9f);
                    trainingContext.ScheduleTrainingRateChange(150, TRAINING_RATE / 27f);
                    trainer.Train(trainingDataProvider, NUM_EPOCHS, trainingContext);
                }
            }
        }
    }
}
