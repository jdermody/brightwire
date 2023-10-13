using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightWire;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;

namespace ExampleCode.DataTableTrainers
{
    internal class IntegerAdditionTrainer : DataTableTrainer
    {
        public IntegerAdditionTrainer(IDataTable data, IDataTable training, IDataTable test) : base(data, training, test)
        {
        }

        public async Task TrainRecurrentNeuralNetwork(bool writeResults = true)
        {
            var graph = _context.CreateGraphFactory();

            // binary classification rounds each output to either 0 or 1
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.GaussianWeightInitialisation(false, 0.3f, GaussianVarianceCalibration.SquareRoot2N))
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.001f, batchSize: 16);

            // build the network
            const int hiddenLayerSize = 20, trainingIterations = 50;
            graph.Connect(engine)
                .AddSimpleRecurrent(graph.ReluActivation(), hiddenLayerSize)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.ReluActivation())
                .AddBackpropagationThroughTime()
            ;

            // train the network for twenty iterations, saving the model on each improvement
            ExecutionGraphModel? bestGraph = null;
            engine.Train(trainingIterations, testData, bn => bestGraph = bn.Graph);

            if (writeResults) {
                // export the graph and verify it against some unseen integers on the best model
                var executionEngine = graph.CreateExecutionEngine(bestGraph ?? engine.Graph);
                var testData2 = graph.CreateDataSource(await BinaryIntegers.Addition(_context, 8));
                var results = executionEngine.Execute(testData2, 128, null, true).ToArray();

                // group the output
                var groupedResults = new (float[][] Input, float[][] Target, float[][] Output)[8];
                for (uint i = 0; i < 8; i++) {
                    var input = new float[32][];
                    var target = new float[32][];
                    var output = new float[32][];
                    for (uint j = 0; j < 32; j++) {
                        input[j] = results[j].Input![i].ToArray();
                        target[j] = results[j].Target![i].ToArray();
                        output[j] = results[j].Output[i].ToArray();
                    }

                    groupedResults[i] = (input, target, output);
                }

                // write the results
                foreach (var (input, target, output) in groupedResults) {
                    Console.Write("First:     ");
                    foreach (var item in input)
                        WriteAsBinary(item[0]);
                    Console.WriteLine();

                    Console.Write("Second:    ");
                    foreach (var item in input)
                        WriteAsBinary(item[1]);
                    Console.WriteLine();
                    Console.WriteLine("           --------------------------------");

                    Console.Write("Expected:  ");
                    foreach (var item in target)
                        WriteAsBinary(item[0]);
                    Console.WriteLine();

                    Console.Write("Predicted: ");
                    foreach (var item in output)
                        WriteAsBinary(item[0]);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        static void WriteAsBinary(float value) => Console.Write(value >= 0.5 ? "1" : "0");
    }
}
