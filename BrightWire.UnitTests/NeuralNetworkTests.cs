using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class NeuralNetworkTests : UnitTestBase
    {
        [Fact]
        public void SimpleLinear()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Float, "x");
            builder.AddColumn(BrightDataType.Float, "y").MetaData.SetTarget(true);
            builder.AddRow(0.1f, 0.2f);
            builder.AddRow(0.2f, 0.4f);
            builder.AddRow(0.3f, 0.6f);
            builder.AddRow(0.4f, 0.8f);
            builder.AddRow(0.5f, 1f);
            using var table = builder.BuildInMemory();
            var graph = _context.CreateGraphFactory();
            var model = graph.TrainSimpleNeuralNetwork(table, table, 
                errorMetric: graph.ErrorMetric.CrossEntropy, 
                learningRate: 0.3f, 
                batchSize: 1, 
                hiddenLayerSize: 4, 
                numIterations: 40, 
                activation: x => x.SigmoidActivation(), 
                gradientDescent: x => x.Adam, 
                weightInitialisation: x => x.Gaussian
            );
            model.Should().NotBeNull();
            var engine = graph.CreateExecutionEngine(model!);
            var result = engine.Execute(new[] { 0.25f }).Single().Output[0][0];
            result.Should().BeInRange(0.45f, 0.55f);
        }
    }
}
