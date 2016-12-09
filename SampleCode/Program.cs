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
            var firstInput = new ConvolutionalData {
                Layers = new[] {
                    new ConvolutionalData.Layer {
                        Height = 2,
                        Width = 2,
                        Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f }
                    }
                },
                ExpectedOutput = new float[] { 1, 0 }
            };
            var emptyInput = new ConvolutionalData {
                Layers = new[] {
                    new ConvolutionalData.Layer {
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
            //var firstPadded = firstInput.AddPadding(PADDING);
            //var emptyPadded = emptyInput.AddPadding(PADDING);

            var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                FieldSize = 2,
                Stride = 2,
                FilterDepth = 3,
                WeightInitialisation = WeightInitialisationType.Gaussian,
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
                var trainingData = new ConvolutionalTrainingDataProvider(lap, 1, convolutionDescriptor, input);

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

                //var firstMatrix = firstPadded.Im2Col(lap, convolutionDescriptor);
                //var emptyMatrix = emptyPadded.Im2Col(lap, convolutionDescriptor);

                //var layer = new ConvolutionalLayer(
                //    lap.NN,
                //    firstMatrix.RowCount,
                //    convolutionDescriptor
                //);
                //var backpropagation = new List<ConvolutionalLayer.Backpropagation>();
                //var output = layer.Execute(firstMatrix, backpropagation);

                //var output2 = output.AsIndexable().ConvertInPlaceToVector();
                //var test2 = output2.ConvertInPlaceToMatrix(output.RowCount, output.ColumnCount);
            }
        }

        static void MNIST(string dataFilesPath)
        {
            Console.Write("Loading training data...");
            var trainingData = Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte");
            var testData = Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte");
            Console.WriteLine($"done - found {trainingData.Count} training samples and {testData.Count} test samples");

            var trainingSamples = trainingData.Shuffle(0).Take(30000).Select(d => d.Convolutional).ToList();
            var convolutionDescriptor = new ConvolutionDescriptor(0.1f) {
                FieldSize = 5,
                Stride = 2,
                FilterDepth = 32,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            const int HIDDEN_SIZE = 128, BATCH_SIZE = 128, NUM_EPOCHS = 200;
            const float TRAINING_RATE = 0.1f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0.1f) {
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var trainingDataProvider = new ConvolutionalTrainingDataProvider(lap, 0, convolutionDescriptor, trainingSamples);

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingDataProvider.InputSize, HIDDEN_SIZE, trainingDataProvider.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.EpochComplete += c => {
                        var output = trainer.Execute(trainingDataProvider);
                        var testError = output.Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                        trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                    };
                    trainer.Train(trainingDataProvider, NUM_EPOCHS, trainingContext);
                }
            }
        }

        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            SimpleTest();
            //MNIST(@"D:\data\mnist\");


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
