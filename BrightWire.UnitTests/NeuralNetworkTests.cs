using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class NeuralNetworkTests : UnitTestBase
    {
        [Fact]
        public async Task SimpleLinear()
        {
            // create a simple table
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Float, "x");
            builder.CreateColumn(BrightDataType.Float, "y").MetaData.SetTarget(true);
            builder.AddRow(0.1f, 0.2f);
            builder.AddRow(0.2f, 0.4f);
            builder.AddRow(0.3f, 0.6f);
            builder.AddRow(0.4f, 0.8f);
            builder.AddRow(0.5f, 1f);
            using var table = await builder.BuildInMemory();

            // train a simple neural network
            var graph = _context.CreateGraphFactory();
            var model = await graph.TrainSimpleNeuralNetwork(table, table, 
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
            var result = (await engine.Execute([0.25f]).First()).Output[0][0];
            result.Should().BeInRange(0.45f, 0.55f);
        }

        [Fact]
        public async Task SimpleLinearNormalised()
        {
            // create a simple table
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Int, "x");
            builder.CreateColumn(BrightDataType.Int, "y").MetaData.SetTarget(true);
            builder.AddRow(1000, 2000);
            builder.AddRow(2000, 4000);
            builder.AddRow(3000, 6000);
            builder.AddRow(4000, 8000);
            builder.AddRow(5000, 10000);
            using var table = await builder.BuildInMemory();

            // normalize the inputs
            using var normalized = await table.Normalize(NormalizationType.FeatureScale);
            var inputNormalization = normalized.GetColumnNormalization(0);
            var outputNormalization = normalized.GetColumnNormalization(1);

            // train a simple neural network
            var graph = _context.CreateGraphFactory();
            var model = await graph.TrainSimpleNeuralNetwork(normalized, normalized, 
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
            var executionResults = engine.Execute([input]);
            var result = (await executionResults.First()).Output;
            var normalizedResult = outputNormalization.ReverseNormalize(result[0][0]);
            normalizedResult.Should().BeInRange(4500, 5500);
        }
    }
}
