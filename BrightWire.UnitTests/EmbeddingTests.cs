using BrightData.LinearAlgebra.VectorIndexing;
using BrightData;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BrightData.UnitTests.Helper;
using FluentAssertions;

namespace BrightWire.UnitTests
{
    public class EmbeddingTests : UnitTestBase
    {
        [Fact]
        public async Task VectorEmbedding()
        {
            var storage = VectorSet<float>.GetStorage(VectorStorageType.InMemory, 4, 3);
            storage.Add([0, 1, 1, 1]);
            storage.Add([0, 0, 0, 0]);
            storage.Add([0, -1, -1, -1]);
            var builder = storage.ToDataTableBuilder(_context);
            using var table = await builder.BuildInMemory();
            table.SetTargetColumn(0);

            // train a vector embedding of size 2
            var graph = _context.CreateGraphFactory();
            var model = await graph.TrainSimpleNeuralNetwork(table, table, 
                errorMetric: graph.ErrorMetric.CrossEntropy, 
                learningRate: 0.01f, 
                batchSize: 1, 
                hiddenLayerSize: 2, 
                numIterations: 5, 
                activation: x => x.TanhActivation(), 
                gradientDescent: x => x.Adam, 
                weightInitialisation: x => x.Gaussian
            );
            var hiddenLayer = model?.OtherNodes.FirstOrDefault(x => x.Name == "hidden");
            var feedForward = (IFeedForward)graph.Create(hiddenLayer!);

            // create a vector index that uses the embedding to compress the vectors
            var index = VectorSet<float>.CreateFromPreCalculatedEmbedding(feedForward.Weight, feedForward.Bias);
            storage.ForEach(x => index.Add(x));

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }
    }
}
