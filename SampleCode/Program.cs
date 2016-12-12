using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.DimensionalityReduction;
using BrightWire.Ensemble;
using BrightWire.Helper;
using BrightWire.Models.Convolutional;
using BrightWire.Models.Input;
using BrightWire.Models.Output;
using BrightWire.TrainingData;
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
        static void SimpleTest()
        {
            const int PADDING = 1;
            var firstInput = new Volume {
                Layers = new[] {
                    new Volume.Layer {
                        Height = 2,
                        Width = 2,
                        Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f }
                    }
                },
                ExpectedOutput = new float[] { 1, 0 }
            };

            var emptyInput = new Volume {
                Layers = new[] {
                    new Volume.Layer {
                        Height = 2,
                        Width = 2,
                        Data = new float[] { 0f, 0f, 0f, 0f }
                    }
                },
                ExpectedOutput = new float[] { 0, 1 }
            };
            var input = new[] {
                firstInput,
                //firstInput,
                emptyInput
            };

            var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                InputSize = 2,
                InputDepth = 1,
                Padding = 1,
                FieldSize = 2,
                Stride = 2,
                FilterDepth = 3,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.Relu
            };

            const int BATCH_SIZE = 2, NUM_EPOCHS = 1000;
            const float LEARNING_RATE = 0.001f;
            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.Relu,
                WeightUpdate = WeightUpdateType.RMSprop,
                WeightInitialisation = WeightInitialisationType.Gaussian
            };
            var errorMetric = ErrorMetricType.OneHot.Create();

            using (var lap = Provider.CreateLinearAlgebra()) {
                var layer = new[] {
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.Extent)
                };
                var trainingData = new ConvolutionalTrainingDataProvider(lap, convolutionDescriptor, input, layer, true);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingData.InputSize, 32, trainingData.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, LEARNING_RATE, BATCH_SIZE);
                    trainingContext.EpochComplete += c => {
                        if (c.CurrentEpoch % 100 == 0) {
                            var output = trainer.Execute(trainingData);
                            var testError = output.Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                            trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                        }
                    };
                    trainer.Train(trainingData, NUM_EPOCHS, trainingContext);
                }
            }
        }

        static void MNIST(string dataFilesPath)
        {
            Console.Write("Loading training data...");
            var trainingData = Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte");
            var testData = Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte");
            Console.WriteLine($"done - found {trainingData.Count} training samples and {testData.Count} test samples");

            var trainingSamples = trainingData.Shuffle(0).Take(60000).Select(d => d.Convolutional).ToList();
            var testSamples = testData.Shuffle(0).Take(10000).Select(d => d.Convolutional).ToList();
            var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                InputSize = 28,
                InputDepth = 1,
                FieldSize = 5,
                Stride = 1,
                Padding = 0,
                FilterDepth = 32,
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
                var layer = new[] {
                    new ConvolutionalLayer(lap.NN, convolutionDescriptor, convolutionDescriptor.Extent),
                    //new ConvolutionalLayer(lap.NN, convolutionDescriptor, 800)
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

        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            //SimpleTest();
            MNIST(@"D:\data\mnist\");

            //IrisClassification();
            //IrisClustering();
            //MarkovChains();
            //MNIST(@"D:\data\mnist\", @"D:\data\mnist\model_test.dat");
            //SentimentClassification(@"D:\data\sentiment labelled sentences\");
            //TextClustering(@"D:\data\[UCI] AAAI-14 Accepted Papers - Papers.csv", @"d:\temp\");
            //IntegerAddition();
            //IncomePrediction(@"d:\data\adult.data", @"d:\data\adult.test");
        }
    }
}
