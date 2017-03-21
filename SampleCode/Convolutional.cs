using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.Models;
using BrightWire.TrainingData;
using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        public static void ReducedMNIST(string dataFilesPath)
        {
            Console.Write("Loading training data...");
            var trainingData = Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte");
            var testData = Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte");
            Console.WriteLine("done");

            var onesAndZeroesTraining = trainingData.Where(s => s.Label == 0 || s.Label == 1).Shuffle(0).Take(1000).ToList();
            var onesAndZeroesTest = testData.Where(s => s.Label == 0 || s.Label == 1).Shuffle(0).Take(100).ToList();

            using (var lap = GPUProvider.CreateLinearAlgebra(false)) {
                var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                    Stride = 1,
                    Padding = 1,
                    FilterDepth = 4,
                    FilterHeight = 3,
                    FilterWidth = 3,
                    WeightInitialisation = WeightInitialisationType.Xavier,
                    WeightUpdate = WeightUpdateType.RMSprop,
                    Activation = ActivationType.LeakyRelu
                };

                const int BATCH_SIZE = 128, NUM_EPOCHS = 2, IMAGE_WIDTH = 28;
                const float TRAINING_RATE = 0.03f;
                var errorMetric = ErrorMetricType.OneHot.Create();
                var layerTemplate = new LayerDescriptor(0.1f) {
                    WeightUpdate = WeightUpdateType.RMSprop,
                    Activation = ActivationType.LeakyRelu
                };

                var trainingSamples = onesAndZeroesTraining.Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var testSamples = onesAndZeroesTest.Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();

                // create a network with a single convolutional layer followed by a max pooling layer
                var convolutionalLayer = new IConvolutionalLayer [] {
                    lap.NN.CreateConvolutionalLayer(convolutionDescriptor, 1, IMAGE_WIDTH, false),
                    lap.NN.CreateMaxPoolingLayer(2, 2, 2)
                };
                var trainingDataProvider = lap.NN.CreateConvolutionalTrainingProvider(convolutionDescriptor, trainingSamples, convolutionalLayer, true);
                var testDataProvider = lap.NN.CreateConvolutionalTrainingProvider(convolutionDescriptor, testSamples, convolutionalLayer, false);

                ConvolutionalNetwork network;
                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, 784, trainingDataProvider.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.EpochComplete += c => {
                        var output = trainer.Execute(testDataProvider.TrainingDataProvider);
                        var testError = output.Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                        trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                    };
                    trainer.Train(trainingDataProvider.TrainingDataProvider, NUM_EPOCHS, trainingContext);

                    network = trainingDataProvider.GetCurrentNetwork(trainer);
                }
                foreach (var layer in convolutionalLayer)
                    layer.Dispose();
                foreach (var item in trainingSamples)
                    item.Item1.Dispose();
                foreach (var item in testSamples)
                    item.Item1.Dispose();

                int correct = 0, total = 0;
                using (var execution = lap.NN.CreateConvolutional(network)) {
                    foreach (var item in onesAndZeroesTest) {
                        using (var tensor = item.AsVolume.AsTensor(lap)) {
                            using (var output = execution.Execute(tensor)) {
                                var maxIndex = output.MaximumIndex();
                                if (maxIndex == item.Label)
                                    ++correct;
                                ++total;
                            }
                        }
                    }
                }
                Console.WriteLine($"Execution results: {(double)correct / total:P0} correct");
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
                FilterDepth = 12,
                FilterHeight = 3,
                FilterWidth = 3,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            const int BATCH_SIZE = 128, NUM_EPOCHS = 20;
            const float TRAINING_RATE = 0.03f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0.1f) {
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var trainingSamples = trainingData.Shuffle(0).Take(1000).Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var testSamples = testData.Shuffle(0).Take(100).Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();

                // create a network with two convolutional layers
                // the first takes the input image of depth 1
                // and the second expects of tensor with depth equal to the output of the first layer
                var layer = new IConvolutionalLayer[] {
                    lap.NN.CreateConvolutionalLayer(convolutionDescriptor, 1, 28),
                    lap.NN.CreateConvolutionalLayer(convolutionDescriptor, convolutionDescriptor.FilterDepth, 28),
                    //lap.NN.CreateConvolutionalLayer(convolutionDescriptor, convolutionDescriptor.FilterDepth, 28),
                };
                var trainingDataProvider = lap.NN.CreateConvolutionalTrainingProvider(convolutionDescriptor, trainingSamples, layer, true);
                var testDataProvider = lap.NN.CreateConvolutionalTrainingProvider(convolutionDescriptor, testSamples, layer, false);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingDataProvider.InputSize, 128, trainingDataProvider.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.EpochComplete += c => {
                        var output = trainer.Execute(testDataProvider.TrainingDataProvider);
                        var testError = output.Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                        trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                    };
                    trainingContext.ScheduleTrainingRateChange(50, TRAINING_RATE / 3f);
                    trainingContext.ScheduleTrainingRateChange(100, TRAINING_RATE / 9f);
                    trainingContext.ScheduleTrainingRateChange(150, TRAINING_RATE / 27f);
                    trainer.Train(trainingDataProvider.TrainingDataProvider, NUM_EPOCHS, trainingContext);
                }
                foreach (var item in layer)
                    item.Dispose();
            }
        }
    }
}
