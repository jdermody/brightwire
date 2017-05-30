using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.TrainingData.Artificial;
using MathNet.Numerics;
using System;
using System.Linq;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        public static void XOR()
        {
            using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
                // Create some training data that the network will learn.  The XOR pattern looks like:
                // 0 0 => 0
                // 1 0 => 1
                // 0 1 => 1
                // 1 1 => 0
                var data = Xor.Get();

                // create the graph (use rmsprop gradient descent optimisation and xavier weight initialisation)
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.Rmse;
                graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                var testData = graph.GetDataSource(data);
                var engine = graph.CreateTrainingEngine(testData, 0.3f, 2);

                // create the network
                const int HIDDEN_LAYER_SIZE = 4;
                graph.Connect(engine)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.SigmoidActivation())
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network
                var executionContext = graph.CreateExecutionContext();
                for (var i = 0; i < 2000; i++) {
                    var trainingError = engine.Train(executionContext);
                    if (i % 100 == 0)
                        engine.Test(testData, errorMetric);
                }
                engine.Test(testData, errorMetric);

                // create a new network to execute the learned network
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);
                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));

                // print the learnt values
                foreach (var item in output) {
                    foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
                        var row = data.GetRow(index);
                        var result = item.Output[index];
                        Console.WriteLine($"{row.GetField<int>(0)} XOR {row.GetField<int>(1)} = {result.Data[0]}");
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            //XOR();
            //IrisClassification();
            //IrisClustering();
            //MarkovChains();
            //MNIST(@"D:\data\mnist\");
            //MNISTConvolutional(@"D:\data\mnist\");
            //SentimentClassification(@"D:\data\sentiment labelled sentences\");
            //TextClustering(@"D:\data\[UCI] AAAI-14 Accepted Papers - Papers.csv", @"d:\temp\");
            IntegerAddition();
            //IncomePrediction(@"d:\data\adult.data", @"d:\data\adult.test");
            //OneToMany();
            //ManyToOne();
            //SequenceToSequence();
        }
    }
}
