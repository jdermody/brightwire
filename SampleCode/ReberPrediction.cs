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

            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.XavierWeightInitialisation())
                ;

                // create the engine
                var trainingData = graph.CreateDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, 0.003f, 32);

                // build the network
                const int HIDDEN_LAYER_SIZE = 64;
                var memory = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine)
                    //.AddSimpleRecurrent(graph.ReluActivation(), memory, "rnn")
                    .AddGru(memory, "rnn")
                    //.AddLstm(memory, "rnn")

                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagationThroughTime(errorMetric)
                ;

                engine.Train(30, testData, errorMetric);
                var networkGraph = engine.Graph;
                var executionContext = graph.CreateExecutionContext();
                var executionEngine = graph.CreateEngine(networkGraph);

                var input = new float[ReberGrammar.Size];
                input[ReberGrammar.GetIndex('B')] = 1f;

                var graphMemory = executionEngine.Start.FindByName("rnn") as IHaveMemoryNode;

                // TODO: generate sample sequence

                //var output = executionEngine.Execute(testData);
                //Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }
    }
}
