using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.Models.Convolutional;
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

            var onesAndZeroesTraining = trainingData.Where(s => s.Label == 0 || s.Label == 1).Take(1000).ToList();
            var onesAndZeroesTest = testData.Where(s => s.Label == 0 || s.Label == 1).Take(100).ToList();

            using (var lap = GPUProvider.CreateLinearAlgebra(false)) {
                var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                    Stride = 1,
                    Padding = 1,
                    FilterDepth = 4,
                    FilterHeight = 3,
                    FilterWidth = 3,
                    WeightInitialisation = WeightInitialisationType.Xavier,
                    WeightUpdate = WeightUpdateType.RMSprop,
                    Activation = ActivationType.Relu
                };

                const int BATCH_SIZE = 32, NUM_EPOCHS = 5, IMAGE_WIDTH = 28;
                const float TRAINING_RATE = 0.1f;
                var errorMetric = ErrorMetricType.OneHot.Create();
                var layerTemplate = new LayerDescriptor(0.1f) {
                    WeightUpdate = WeightUpdateType.RMSprop,
                    Activation = ActivationType.Relu
                };

                var trainingSamples = onesAndZeroesTraining.Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var testSamples = onesAndZeroesTest.Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var convolutionalLayer = new IConvolutionalLayer [] {
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.FilterSize, IMAGE_WIDTH),
                    //new MaxPoolingLayer(lap, convolutionDescriptor, 2, 2, IMAGE_WIDTH)
                };
                var trainingDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, trainingSamples, convolutionalLayer, true);
                var testDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, testSamples, convolutionalLayer, false);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, 784 * 4, trainingDataProvider.OutputSize)) {
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
                foreach (var layer in convolutionalLayer)
                    layer.Dispose();
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
                FilterDepth = 4,
                FilterHeight = 3,
                FilterWidth = 3,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            const int BATCH_SIZE = 128, NUM_EPOCHS = 200;
            const float TRAINING_RATE = 0.03f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0.1f) {
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var trainingSamples = trainingData.Shuffle(0).Take(60000).Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var testSamples = testData.Shuffle(0).Take(10000).Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var layer = new IConvolutionalLayer[] {
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.FilterSize, 28),
                    //new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.FilterSize * convolutionDescriptor.FilterDepth, 28)
                    new MaxPoolingLayer(lap, convolutionDescriptor, 2, 2, 28)
                };
                var trainingDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, trainingSamples, layer, true);
                var testDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, testSamples, layer, false);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingDataProvider.InputSize / 4, 128, trainingDataProvider.OutputSize)) {
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
                foreach (var item in layer)
                    item.Dispose();
            }
        }
    }
}
