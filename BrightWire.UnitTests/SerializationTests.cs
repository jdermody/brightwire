﻿using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.UnitTests.Helper;
using BrightWire.ExecutionGraph;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class SerializationTests : CpuBase
    {
        public class CustomErrorMetric : IErrorMetric
        {
            public float Compute(float[] output, float[] targetOutput)
            {
                return output.MaximumIndex() == targetOutput.MaximumIndex() ? 1 : 0;
            }

            public IMatrix<float> CalculateGradient(IMatrix<float> output, IMatrix<float> targetOutput)
            {
                return targetOutput.Subtract(output);
            }

            public float Compute(IReadOnlyVector<float> output, IReadOnlyVector<float> targetOutput)
            {
                return output.GetMinAndMaxValues().MaxIndex == targetOutput.GetMinAndMaxValues().MaxIndex ? 1 : 0;
            }

            public bool DisplayAsPercentage => true;
        }

        static GraphModel? _bestNetwork = null;

        static async Task<(GraphFactory, IDataSource)> MakeGraphAndData(BrightDataContext context)
        {
            var graph = new GraphFactory(context.LinearAlgebraProvider);
            var data = await graph.CreateDataSource(await And.Get(context));
            return (graph, data);
        }

        public SerializationTests()
        {
            var (graph, data) = MakeGraphAndData(_context).Result;
            var errorMetric = new CustomErrorMetric();
            var engine = graph.CreateTrainingEngine(data, errorMetric);

            graph.Connect(engine)
                .AddFeedForward(1)
                .Add(graph.SigmoidActivation())
                .AddBackpropagation();
            var model = engine.Train(400, data, bn => _bestNetwork = bn).Result;

            var executionEngine = graph.CreateExecutionEngine(_bestNetwork!.Graph);
            AssertEngineGetsGoodResults(executionEngine, data);
        }

        static void AssertEngineGetsGoodResults(IGraphExecutionEngine engine, IDataSource data)
        {
            var results = engine.Execute(data).ToBlockingEnumerable().FirstOrDefault();
            results.Should().NotBeNull();
            static bool Handle(IReadOnlyVector<float> value) => value[0] > 0.5f;
            var zippedResults = results.Output.Zip(results.Target!, (result, target) => Handle(result) == Handle(target));
            zippedResults.All(x => x).Should().BeTrue();
        }

        [Fact]
        public async Task CreateFromExecutionGraph()
        {
            var (graph, data) = await MakeGraphAndData(_context);
            var engine = graph.CreateExecutionEngine(_bestNetwork!.Graph);
            AssertEngineGetsGoodResults(engine, data);
        }

        //[Fact]
        //public void DeserialiseExecutionGraph()
        //{
        //    var (graph, data) = MakeGraphAndData(_context);
        //    ExecutionGraph executionGraphReloaded = null;

        //    using (var file = new MemoryStream())
        //    {
        //        Serializer.Serialize(file, bestNetwork.Graph);
        //        file.Position = 0;
        //        executionGraphReloaded = Serializer.Deserialize<ExecutionGraph>(file);
        //    }
        //    Assert.IsNotNull(executionGraphReloaded);
        //    var engine = graph.CreateEngine(executionGraphReloaded);
        //    AssertEngineGetsGoodResults(engine, data);
        //}

        //void _AssertEqual<T>(T[] array1, T[] array2)
        //{
        //    array1.Should().HaveSameCount(array2);
        //    for (var i = 0; i < array1.Length; i++)
        //        array1[i].Should().Be(array2[i]);
        //}

        //[Fact]
        //public void DeserialiseVectorisationModel()
        //{
        //    var builder = BrightWireProvider.CreateDataTableBuilder();
        //    builder.AddColumn(ColumnType.String, "label");
        //    builder.AddColumn(ColumnType.String, "output", true);

        //    builder.Add("a", "0");
        //    builder.Add("b", "0");
        //    builder.Add("c", "1");

        //    var dataTable = builder.Build();
        //    var vectoriser = dataTable.GetVectoriser();
        //    var model = vectoriser.GetVectorisationModel();

        //    var vectorList = new List<FloatVector>();
        //    dataTable.ForEach(row => vectorList.Add(vectoriser.GetInput(row)));

        //    var vectoriser2 = dataTable.GetVectoriser(model);
        //    var vectorList2 = new List<FloatVector>();
        //    dataTable.ForEach(row => vectorList2.Add(vectoriser2.GetInput(row)));

        //    foreach (var item in vectorList.Zip(vectorList2, (v1, v2) => (v1, v2)))
        //        _AssertEqual(item.Item1.Data, item.Item2.Data);
        //}
    }
}
