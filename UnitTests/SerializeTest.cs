using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

namespace UnitTests
{

    [TestClass]
    public class SerializeTest
    {
        public class CustomErrorMatrix : IErrorMetric
        {
            public IMatrix CalculateGradient(IContext context, IMatrix output, IMatrix targetOutput)
            {
                return targetOutput.Subtract(output);
            }

            public float Compute(FloatVector output, FloatVector targetOutput)
            {
                return output.MaximumIndex() == targetOutput.MaximumIndex() ? 1 : 0;
            }

            public bool DisplayAsPercentage => true;
        }

        static ILinearAlgebraProvider _cpu;
        private static GraphModel bestNetwork;

        static (GraphFactory, IDataSource) MakeGraphAndData()
        {
            var graph = new GraphFactory(_cpu);
            var data = graph.CreateDataSource(And.Get());
            return (graph, data);
        }

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _cpu = BrightWireProvider.CreateLinearAlgebra(false);

            var (graph, data) = MakeGraphAndData();
            var engine = graph.CreateTrainingEngine(data);

            var errorMetric = new CustomErrorMatrix();
            graph.Connect(engine)
                .AddFeedForward(1)
                .Add(graph.SigmoidActivation())
                .AddBackpropagation(errorMetric);
            engine.Train(300, data, errorMetric, bn => bestNetwork = bn);
            AssertEngineGetsGoodResults(engine, data);
        }

        private static void AssertEngineGetsGoodResults(IGraphEngine engine, IDataSource data)
        {
            var results = engine.Execute(data)?.FirstOrDefault();
            Assert.IsNotNull(results);
            bool Handle(FloatVector value) => value.Data[0] > 0.5f;
            Debug.Assert(results.Output.Zip(results.Target, (result, target) => Handle(result) == Handle(target)).All(x => x));
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _cpu.Dispose();
        }

        [TestMethod]
        public void CreateFromExcuationGraph()
        {
            var (graph, data) = MakeGraphAndData();
            var engine = graph.CreateEngine(bestNetwork.Graph);
            AssertEngineGetsGoodResults(engine, data);
        }

        [TestMethod]
        public void DeserlizeExcuationGraph()
        {
            var (graph, data) = MakeGraphAndData();
            ExecutionGraph executionGraphReloaded = null;

            using (var file = new MemoryStream())
            {
                Serializer.Serialize(file, bestNetwork.Graph);
                file.Position = 0;
                executionGraphReloaded = Serializer.Deserialize<ExecutionGraph>(file);
            }
            Assert.IsNotNull(executionGraphReloaded);
            var engine = graph.CreateEngine(executionGraphReloaded);
            AssertEngineGetsGoodResults(engine, data);
        }

    }
}
