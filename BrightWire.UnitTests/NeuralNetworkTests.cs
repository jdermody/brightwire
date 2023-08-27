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
            // create a simple table
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Float, "x");
            builder.AddColumn(BrightDataType.Float, "y").MetaData.SetTarget(true);
            builder.AddRow(0.1f, 0.2f);
            builder.AddRow(0.2f, 0.4f);
            builder.AddRow(0.3f, 0.6f);
            builder.AddRow(0.4f, 0.8f);
            builder.AddRow(0.5f, 1f);
            using var table = builder.BuildInMemory();

            // train a simple neural network
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

            // test the model
            model.Should().NotBeNull();
            var engine = graph.CreateExecutionEngine(model!);
            var result = engine.Execute(new[] { 0.25f }).Single().Output[0][0];
            result.Should().BeInRange(0.45f, 0.55f);
        }

        [Fact]
        public void SimpleLinearNormalised()
        {
            // create a simple table
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Int, "x");
            builder.AddColumn(BrightDataType.Int, "y").MetaData.SetTarget(true);
            builder.AddRow(1000, 2000);
            builder.AddRow(2000, 4000);
            builder.AddRow(3000, 6000);
            builder.AddRow(4000, 8000);
            builder.AddRow(5000, 10000);
            using var table = builder.BuildInMemory();

            // normalize the inputs
            using var normalized = table.Normalize(NormalizationType.FeatureScale);
            var inputNormalization = normalized.GetColumnNormalization(0);
            var outputNormalization = normalized.GetColumnNormalization(1);

            // train a simple neural network
            var graph = _context.CreateGraphFactory();
            var model = graph.TrainSimpleNeuralNetwork(normalized, normalized, 
                errorMetric: graph.ErrorMetric.CrossEntropy, 
                learningRate: 0.1f, 
                batchSize: 1, 
                hiddenLayerSize: 4, 
                numIterations: 40, 
                activation: x => x.SigmoidActivation(), 
                gradientDescent: x => x.Adam, 
                weightInitialisation: x => x.Gaussian
            );

            // test the model
            model.Should().NotBeNull();
            var engine = graph.CreateExecutionEngine(model!);
            var input = (float)inputNormalization.Normalize(2500);
            var result = engine.Execute(new[] { input }).Single().Output;
            var normalizedResult = outputNormalization.ReverseNormalize(result[0][0]);
            normalizedResult.Should().BeInRange(4500, 5500);
        }
    }
}
