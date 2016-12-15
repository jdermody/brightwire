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

            var onesAndZeroesTraining = trainingData.Where(s => s.Label == 0 || s.Label == 1).ToList();
            var onesAndZeroesTest = testData.Where(s => s.Label == 0 || s.Label == 1).ToList();

            using (var lap = Provider.CreateLinearAlgebra()) {
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

                const int BATCH_SIZE = 32, NUM_EPOCHS = 200, IMAGE_WIDTH = 28;
                const float TRAINING_RATE = 0.03f;
                var errorMetric = ErrorMetricType.OneHot.Create();
                var layerTemplate = new LayerDescriptor(0.1f) {
                    WeightUpdate = WeightUpdateType.RMSprop,
                    Activation = ActivationType.Relu
                };

                var trainingSamples = onesAndZeroesTraining.Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var testSamples = onesAndZeroesTest.Select(d => d.AsVolume).Select(d => Tuple.Create(d.AsTensor(lap), d.ExpectedOutput)).ToList();
                var convolutionalLayer = new IConvolutionalLayer [] {
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.FilterSize, IMAGE_WIDTH),
                    new MaxPoolingLayer(lap, convolutionDescriptor, 2, 2, IMAGE_WIDTH)
                };
                var trainingDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, trainingSamples, convolutionalLayer, true);
                var testDataProvider = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, testSamples, convolutionalLayer, false);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, 784, trainingDataProvider.OutputSize)) {
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
