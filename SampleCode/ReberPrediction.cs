using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
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
        static void ReberPrediction()
        {
            var grammar = new ReberGrammar(false);
            var sequences = grammar.GetExtended(10).Take(500).ToList();
            var data = ReberGrammar.GetOneHot(sequences).Split(0);

            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                var trainingData = graph.CreateDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, 0.1f, 32);

                // build the network
                const int HIDDEN_LAYER_SIZE = 64;
                var memory = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine)
                    //.AddSimpleRecurrent(graph.ReluActivation(), memory)
                    .AddGru(memory)
                    //.AddRan(memory)
                    //.AddLstm(memory)

                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.TanhActivation())
                    .AddBackpropagationThroughTime(errorMetric)
                ;

                engine.Train(30, testData, errorMetric);

                // generate sample sequence
                var networkGraph = engine.Graph;
                using (var executionContext = graph.CreateExecutionContext()) {
                    var executionEngine = graph.CreateEngine(networkGraph);

                    Console.WriteLine("Generating a new reber sequence...");
                    var input = new float[ReberGrammar.Size];
                    input[ReberGrammar.GetIndex('B')] = 1f;
                    Console.Write("B");

                    int index = 0, eCount = 0;
                    var result = executionEngine.ExecuteSequential(index++, input, executionContext, MiniBatchSequenceType.SequenceStart);
                    for (var i = 0; i < 32; i++) {
                        var nextIndex = result.Output[0].Data
                            .Select((v, j) => (v, j))
                            .Where(d => d.Item1 >= 0.5f)
                            .Select(d => d.Item2)
                            .Shuffle()
                            .FirstOrDefault()
                        ;
                        if (index == 0)
                            break;

                        Console.Write(ReberGrammar.GetChar(nextIndex));
                        if (nextIndex == ReberGrammar.GetIndex('E') && ++eCount == 2)
                            break;

                        input = new float[ReberGrammar.Size];
                        input[nextIndex] = 1f;
                        result = executionEngine.ExecuteSequential(index++, input, executionContext, MiniBatchSequenceType.Standard);
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
