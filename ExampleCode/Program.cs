using BrightWire;
using BrightWire.Descriptor.GradientDescent;
using BrightWire.Descriptor.WeightInitialisation;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Component;
using BrightWire.ExecutionGraph.ErrorMetric;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Layer;
using BrightWire.ExecutionGraph.Wire;
using BrightWire.TrainingData.Artificial;
using MathNet.Numerics;
using MathNet.Numerics.Providers.Common.Mkl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExampleCode
{
    class Program
    {
        static void XorTest()
        {
            using (var lap = Provider.CreateLinearAlgebra()) {
                var data = Xor.Get();

                // 
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.Rmse;
                graph.CurrentPropertySet
                    .Use(new RmsPropDescriptor(0.9f))
                    .Use(new XavierDescriptor())
                ;

                var trainingProvider = graph.CreateMiniBatchProvider(data);
                var engine = graph.CreateTrainingEngine(0.03f, 2, graph.CreateGraphInput(trainingProvider));

                const int HIDDEN_LAYER_SIZE = 4;
                var network = graph.Connect(engine.Input)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.Activation.Sigmoid)
                    .AddFeedForward(null)
                    .Add(graph.Activation.Sigmoid)
                    .AddAction(new Backpropagate(errorMetric))
                    .Build()
                ;

                for (var i = 0; i < 2000; i++) {
                    var trainingError = engine.Train(trainingProvider);
                    if (i % 100 == 0)
                        engine.WriteTestResults(trainingProvider, errorMetric);
                }
                engine.WriteTestResults(trainingProvider, errorMetric);

                var testData = data.SelectColumns(Enumerable.Range(0, 2));
                var testProvider = graph.CreateMiniBatchProvider(testData, testData.GetVectoriser(false));
                var executionEngine = graph.CreateEngine(testProvider, engine.Input);
                var results = executionEngine.Execute();

                for(var i = 0; i < results.Count; i++) {
                    var row = testData.GetRow(i);
                    var result = results[i];

                    Console.WriteLine($"{row.GetField<int>(0)} XOR {row.GetField<int>(1)} = {result[0]}");
                }
            }
        }

        static void IrisTest()
        {
            var data = new StreamReader(new FileStream(@"D:\data\iris.txt", FileMode.Open)).ParseCSV().Split(0);
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.OneHotEncoding;

                // create the property set
                graph.CurrentPropertySet
                    .Use(new AdamDescriptor())
                ;

                // create the traiing data
                var trainingData = graph.CreateMiniBatchProvider(data.Training);
                var testData = graph.CreateMiniBatchProvider(data.Test);
                var engine = graph.CreateTrainingEngine(0.01f, 32, graph.CreateGraphInput(trainingData));

                const int HIDDEN_LAYER_SIZE = 4;
                var network = graph.Connect(engine.Input)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.Activation.Sigmoid)
                    .AddFeedForward()
                    .Add(graph.Activation.SoftMax)
                    .AddAction(new Backpropagate(errorMetric))
                    .Build()
                ;

                for (var i = 0; i < 1000; i++) {
                    var trainingError = engine.Train(trainingData);
                    if (i % 100 == 0)
                        engine.WriteTestResults(testData, errorMetric);
                }
                engine.WriteTestResults(testData, errorMetric);
            }
        }

        static void IntegerAddition()
        {
            var data = BinaryIntegers.Addition(1000, false).Split(0);
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;

                // create the property set
                graph.CurrentPropertySet
                    .Use(new RmsPropDescriptor())
                    .Use(new XavierDescriptor())
                ;

                // create the engine
                var trainingData = graph.CreateMiniBatchProvider(data.Training);
                var testData = graph.CreateMiniBatchProvider(data.Test);
                var engine = graph.CreateTrainingEngine(0.003f, 8, graph.CreateGraphInput(trainingData));
                engine.Context.EnableLogging = false;

                // build the network
                const int HIDDEN_LAYER_SIZE = 32;
                var memory = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine.Input)
                    //.AddSimpleRecurrent(graph.Activation.Relu, memory)
                    .AddGru(memory)
                    .AddFeedForward(engine.Input.OutputSize)
                    .Add(graph.Activation.Tanh)
                    .AddAction(new Backpropagate(errorMetric))
                    .Build()
                ;

                for (var i = 0; i < 25; i++) {
                    var trainingError = engine.Train(trainingData);
                    engine.WriteTestResults(testData, errorMetric);
                }

                //var verificationData = graph.CreateMiniBatchProvider(BinaryIntegers.Addition(8, true));
                //var engine2 = graph.CreateEngine(verificationData, graph.CreateGraphInput(verificationData));
                //var output = engine2.Execute();
            }
        }

        static void Main(string[] args)
        {
            Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Single, MklAccuracy.Low);
            //var matrix = lap.Create(3, 3, (i, j) => (i+1) * (j+1));
            //var rot180 = matrix.Rotate180();

            //var xml = matrix.AsIndexable().AsXml;
            //var xml2 = rot180.AsIndexable().AsXml;

            //XorTest();
            //IrisTest();
            IntegerAddition();
        }
    }
}