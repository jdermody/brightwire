using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightWire;
using BrightWire.Models;

namespace ExampleCode.DataTableTrainers
{
    internal class XorTrainer : DataTableTrainer
    {
        public XorTrainer(IDataTable table) : base(table, table, table)
        {
        }

        public async Task<ExecutionGraphModel?> Train(uint hiddenLayerSize, uint numIterations, float learningRate, uint batchSize, bool writeResults = true)
        {
            var targetColumnIndex = Table.Value.GetTargetColumnOrThrow();

            // train a model
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;
            var model = graph.TrainSimpleNeuralNetwork(
                Training, 
                Test, 
                errorMetric, 
                learningRate, 
                batchSize, 
                hiddenLayerSize, 
                numIterations, 
                g => g.SigmoidActivation(), 
                g => g.RmsProp, 
                g => g.Gaussian
            );

            if (model != null) {
                // create a new network to execute the learned network
                var executionEngine = graph.CreateExecutionEngine(model);
                var output = executionEngine.Execute(graph.CreateDataSource(Test)).ToList();
                if (writeResults) {
                    var testAccuracy = output.Average(o => o.CalculateError(errorMetric));
                    Console.WriteLine($"Neural network accuracy: {testAccuracy:P}");
                    var testData = await Test.GetAllRows();

                    // print the values that have been learned
                    foreach (var item in output) {
                        foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
                            var row = testData[index];
                            var result = item.Output[index];
                            var input = row.ToArray()
                                .Select((v, i) => (Val: v, Ind: i))
                                .Where(d => d.Ind != targetColumnIndex)
                                .Select(d => d.Val)
                                .AsCommaSeparated()
                            ;
                            Console.WriteLine($"{input} = {result.ToArray().AsCommaSeparated()}");
                        }
                    }
                }
            }

            return model;
        }
    }
}
