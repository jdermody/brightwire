using BrightWire.ExecutionGraph;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using System;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static void _WriteAsBinary(float value)
        {
            if (value >= 0.5)
                Console.Write("1");
            else
                Console.Write("0");
        }

        public static void IntegerAddition()
        {
            // generate 1000 random integer additions (split into training and test sets)
            var data = BinaryIntegers.Addition(1000, false).Split(0);
            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);

                // binary classification rounds each output to either 0 or 1
                var errorMetric = graph.ErrorMetric.BinaryClassification;

                // configure the network properties
                graph.CurrentPropertySet
                    .Use(graph.GradientDescent.Adam)
                    .Use(graph.GaussianWeightInitialisation(false, 0.1f, GaussianVarianceCalibration.SquareRoot2N))
                ;

                // create the engine
                var trainingData = graph.CreateDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, learningRate: 0.01f, batchSize: 16);

                // build the network
                const int HIDDEN_LAYER_SIZE = 32, TRAINING_ITERATIONS = 30;
                graph.Connect(engine)
                    .AddSimpleRecurrent(graph.ReluActivation(), HIDDEN_LAYER_SIZE)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.ReluActivation())
                    .AddBackpropagationThroughTime(errorMetric)
                ;

                // train the network for twenty iterations, saving the model on each improvement
                Models.ExecutionGraph bestGraph = null;
                engine.Train(TRAINING_ITERATIONS, testData, errorMetric, bn => bestGraph = bn.Graph);

                // export the graph and verify it against some unseen integers on the best model
                var executionEngine = graph.CreateEngine(bestGraph ?? engine.Graph);
                var testData2 = graph.CreateDataSource(BinaryIntegers.Addition(8, true));
                var results = executionEngine.Execute(testData2);

                // group the output
                var groupedResults = new (FloatVector[] Input, FloatVector[] Target, FloatVector[] Output)[8];
                for(var i = 0; i < 8; i++) {
                    var input = new FloatVector[32];
                    var target = new FloatVector[32];
                    var output = new FloatVector[32];
                    for(var j = 0; j < 32; j++) {
                        input[j] = results[j].Input[0][i];
                        target[j] = results[j].Target[i];
                        output[j] = results[j].Output[i];
                    }
                    groupedResults[i] = (input, target, output);
                }

                // write the results
                foreach (var result in groupedResults) {
                    Console.Write("First:     ");
                    foreach (var item in result.Input)
                        _WriteAsBinary(item.Data[0]);
                    Console.WriteLine();

                    Console.Write("Second:    ");
                    foreach (var item in result.Input)
                        _WriteAsBinary(item.Data[1]);
                    Console.WriteLine();
                    Console.WriteLine("           --------------------------------");

                    Console.Write("Expected:  ");
                    foreach (var item in result.Target)
                        _WriteAsBinary(item.Data[0]);
                    Console.WriteLine();

                    Console.Write("Predicted: ");
                    foreach (var item in result.Output)
                        _WriteAsBinary(item.Data[0]);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
    }
}
