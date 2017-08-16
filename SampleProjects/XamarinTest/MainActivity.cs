using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using BrightWire;
using BrightWire.TrainingData.Artificial;
using BrightWire.ExecutionGraph;
using System.Linq;
using System.Text;

namespace XamarinTest
{
    [Activity(Label = "Xamarin Test", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var sb = new StringBuilder();
            using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
                // Create some training data that the network will learn.  The XOR pattern looks like:
                // 0 0 => 0
                // 1 0 => 1
                // 0 1 => 1
                // 1 1 => 0
                var data = Xor.Get();

                // create the graph
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.CrossEntropy;
                graph.CurrentPropertySet
                    // use rmsprop gradient descent optimisation
                    .Use(graph.GradientDescent.RmsProp)

                    // and xavier weight initialisation
                    .Use(graph.WeightInitialisation.Gaussian)
                ;

                // create the engine
                var testData = graph.CreateDataSource(data);
                var engine = graph.CreateTrainingEngine(testData, 0.1f, 4);

                // create the network
                const int HIDDEN_LAYER_SIZE = 6;
                graph.Connect(engine)
                    // create a feed forward layer with sigmoid activation
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.SigmoidActivation())

                    // create a second feed forward layer with sigmoid activation
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())

                    // backpropagate the error signal at the end of the graph
                    .AddBackpropagation(errorMetric)
                ;

                // train the network
                var executionContext = graph.CreateExecutionContext();
                for (var i = 0; i < 1000; i++) {
                    var trainingError = engine.Train(executionContext);
                    if (i % 100 == 0)
                        engine.Test(testData, errorMetric);
                }
                engine.Test(testData, errorMetric);

                // create a new network to execute the learned network
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);
                var output = executionEngine.Execute(testData);

                // print the learnt values
                foreach (var item in output) {
                    foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
                        var row = data.GetRow(index);
                        var result = item.Output[index];
                        sb.AppendLine($"{row.GetField<int>(0)} XOR {row.GetField<int>(1)} = {result.Data[0]}");
                    }
                }
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var outputText = FindViewById<TextView>(Resource.Id.outputText);
            outputText.Text = sb.ToString();
        }
    }
}

