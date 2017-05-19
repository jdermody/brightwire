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
                var errorMetric = graph.ErrorMetric.Rmse;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.XavierWeightInitialisation())
                //.Use(graph.L2(0.1f))
                ;

                // create the engine
                var trainingData = graph.GetDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var executionContext = graph.CreateExecutionContext();
                var engine = graph.CreateTrainingEngine(trainingData, executionContext, 0.003f, 32);

                // build the network
                const int HIDDEN_LAYER_SIZE = 64;
                var memory = new float[HIDDEN_LAYER_SIZE];
                //var memory2 = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine)
                    .AddSimpleRecurrent(graph.ReluActivation(), memory)
                    //.AddGru(memory)
                    //.AddLstm(memory)

                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddForwardAction(new BackpropagateThroughTime(errorMetric))
                    .Build()
                ;

                engine.Train(30, testData, errorMetric);
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph, executionContext);

                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }
    }
}
