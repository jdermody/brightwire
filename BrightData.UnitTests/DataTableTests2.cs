using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableTests2
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public async Task InMemoryColumnOriented()
        {
            var builder = _context.CreateTableBuilder();
            var stringColumnBuilder = builder.CreateColumn<string>("string column");
            stringColumnBuilder.Append("a row");
            stringColumnBuilder.Append("another row");

            var intColumnBuilder = builder.CreateColumn<int>("int column");
            intColumnBuilder.Append(123);
            intColumnBuilder.Append(234);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);

            var stringReader = await dataTable.GetColumn<string>(0).ToArray();
            stringReader.Length.Should().Be(2);

            var intReader = await dataTable.GetColumn<int>(1).ToArray();
            intReader.Length.Should().Be(2);

            var str = dataTable.Get<string>(0, 0);
            var str2 = dataTable.Get<string>(0, 1);
            var val1 = dataTable.Get<int>(1, 0);
            var val2 = dataTable.Get<int>(1, 1);

            var row1 = dataTable[0];
            var row2 = dataTable[1];
        }

        [Fact]
        public async Task InMemoryIndexList()
        {
            var builder = _context.CreateTableBuilder();
            var indexListBuilder = builder.CreateColumn<IndexList>("index list");
            var sampleIndexList = IndexList.Create(1, 2, 3, 4, 5);
            indexListBuilder.Append(sampleIndexList);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);

            var fromTable = await dataTable.Get<IndexList>(0, 0);
            fromTable.Should().BeEquivalentTo(sampleIndexList);
        }

        [Fact]
        public async Task InMemoryWeightedIndexList()
        {
            var builder = _context.CreateTableBuilder();
            var indexListBuilder = builder.CreateColumn<WeightedIndexList>("weighted index list");
            var sampleIndexList = WeightedIndexList.Create((1, 1f), (2, 0.1f), (3, 0.75f), (4, 0.25f), (5, 0.77f));
            indexListBuilder.Append(sampleIndexList);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);

            var fromTable = await dataTable.Get<WeightedIndexList>(0, 0);
            fromTable.Should().BeEquivalentTo(sampleIndexList);
        }

        void CheckSame(IHaveReadOnlyTensorSegment<float> tensorSegment1, IHaveReadOnlyTensorSegment<float> tensorSegment2)
        {
            tensorSegment1.ReadOnlySegment.Values.Should().BeEquivalentTo(tensorSegment2.ReadOnlySegment.Values);
        }

        [Fact]
        public async Task InMemoryVector()
        {
            var builder = _context.CreateTableBuilder();
            var vectorBuilder = builder.CreateColumn<ReadOnlyVector>("vector");
            var firstVector = _context.CreateReadOnlyVector(5, i => i + 1);
            vectorBuilder.Append(firstVector);
            var secondVector = _context.CreateReadOnlyVector(5, i => i + 2);
            vectorBuilder.Append(secondVector);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);
            var column = dataTable.GetColumn<ReadOnlyVector>(0);
            var columnItems = await column.ToArray();

            using var fromTable = columnItems[0].Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstVector);
            using var fromTable2 = columnItems[1].Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable2, secondVector);
        }

        [Fact]
        public async Task InMemoryMatrix()
        {
            var builder = _context.CreateTableBuilder();
            var matrixBuilder = builder.CreateColumn<ReadOnlyMatrix>("matrix");
            var firstMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j);
            matrixBuilder.Append(firstMatrix);
            var secondMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => (i + j) + 1);
            matrixBuilder.Append(secondMatrix);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);

            var column = dataTable.GetColumn<ReadOnlyMatrix>(0);
            var columnItems = await column.ToArray();
            columnItems.Last().Should().BeEquivalentTo(secondMatrix);

            using var fromTable = columnItems[0].Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstMatrix);
            var fromTable2 = columnItems[1];
            CheckSame(fromTable2, secondMatrix);

            dataTable[1][0].Should().BeEquivalentTo(secondMatrix);
        }

        [Fact]
        public async Task InMemoryTensor3D()
        {
            var builder = _context.CreateTableBuilder();
            var tensorBuilder = builder.CreateColumn<ReadOnlyTensor3D>("tensor");
            var firstTensor = _context.CreateReadOnlyTensor3D(
                _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j),
                _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j)
            );
            tensorBuilder.Append(firstTensor);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);

            using var fromTable = (await dataTable.Get<ReadOnlyTensor3D>(0, 0)).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstTensor);
        }

        [Fact]
        public async Task InMemoryTensor4D()
        {
            var builder = _context.CreateTableBuilder();
            var tensorBuilder = builder.CreateColumn<ReadOnlyTensor4D>("tensor");
            var firstTensor = _context.CreateReadOnlyTensor4D(
                _context.CreateReadOnlyTensor3D(
                    _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j),
                    _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j)
                ),
                _context.CreateReadOnlyTensor3D(
                    _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j),
                    _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j)
                )
            );
            tensorBuilder.Append(firstTensor);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = _context.LoadTableFromStream(stream);

            using var fromTable = (await dataTable.Get<ReadOnlyTensor4D>(0, 0)).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstTensor);
        }
    }
}
