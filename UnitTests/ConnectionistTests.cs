using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.Helper;
using BrightWire.LinearAlgebra;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Helper;

namespace UnitTests
{
    [TestClass]
    public class ConnectionistTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = BrightWireProvider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void TiedAutoEncoder()
        {
            const int DATA_SIZE = 1000, REDUCED_SIZE = 200;

            // create some random data
            var rand = new Random();
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Vector, "Input");
            builder.AddColumn(ColumnType.Vector, "Output", true);
            for(var i = 0; i < 100; i++) {
                var vector = new FloatVector {
                    Data = Enumerable.Range(0, DATA_SIZE).Select(j => Convert.ToSingle(rand.NextDouble())).ToArray()
                };
                builder.Add(vector, vector);
            }
            var dataTable = builder.Build();

            // build the autoencoder with tied weights
            var graph = new GraphFactory(_lap);
            var dataSource = graph.CreateDataSource(dataTable);
            var engine = graph.CreateTrainingEngine(dataSource, 0.03f, 32);
            var errorMetric = graph.ErrorMetric.Quadratic;
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
                .Use(graph.WeightInitialisation.Xavier)
            ;

            graph.Connect(engine)
                .AddFeedForward(REDUCED_SIZE, "layer")
                .Add(graph.TanhActivation())
                .AddTiedFeedForward(engine.Input.FindByName("layer") as IFeedForward)
                .Add(graph.TanhActivation())
                .AddBackpropagation(errorMetric)
            ;
            using (var executionContext = graph.CreateExecutionContext()) {
                for (var i = 0; i < 2; i++) {
                    var trainingError = engine.Train(executionContext);
                }
            }
            var networkGraph = engine.Graph;
            var executionEngine = graph.CreateEngine(networkGraph);
        }
    }
}
