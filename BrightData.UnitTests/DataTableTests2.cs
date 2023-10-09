using System.IO;
using System.Linq;
using BrightData.DataTable;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableTests2
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public void InMemoryColumnOriented()
        {
            var builder = _context.CreateTableBuilder();
            var stringColumnBuilder = builder.AddColumn<string>("string column");
            stringColumnBuilder.Add("a row");
            stringColumnBuilder.Add("another row");

            var intColumnBuilder = builder.AddColumn<int>("int column");
            intColumnBuilder.Add(123);
            intColumnBuilder.Add(234);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var stringReader = dataTable.ReadColumn<string>(0);
            stringReader.Values.Count().Should().Be(2);
            stringReader.Values.Count().Should().Be(2);

            using var intReader = dataTable.ReadColumn<int>(1);
            intReader.Values.Count().Should().Be(2);
            intReader.Values.Count().Should().Be(2);

            var str = dataTable.Get<string>(0, 0);
            var str2 = dataTable.Get<string>(1, 0);
            var val1 = dataTable.Get<int>(0, 1);
            var val2 = dataTable.Get<int>(1, 1);

            var row1 = dataTable.GetRow(0);
            var row2 = dataTable.GetRow(1);
        }

        [Fact]
        public void InMemoryIndexList()
        {
            var builder = _context.CreateTableBuilder();
            var indexListBuilder = builder.AddColumn<IndexList>("index list");
            var sampleIndexList = _context.CreateIndexList(1, 2, 3, 4, 5);
            indexListBuilder.Add(sampleIndexList);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            var fromTable = dataTable.Get<IndexList>(0, 0);
            fromTable.Should().BeEquivalentTo(sampleIndexList, options => options.ComparingByMembers<IndexList>());
        }

        [Fact]
        public void InMemoryWeightedIndexList()
        {
            var builder = _context.CreateTableBuilder();
            var indexListBuilder = builder.AddColumn<WeightedIndexList>("weighted index list");
            var sampleIndexList = _context.CreateWeightedIndexList((1, 1f), (2, 0.1f), (3, 0.75f), (4, 0.25f), (5, 0.77f));
            indexListBuilder.Add(sampleIndexList);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            var fromTable = dataTable.Get<WeightedIndexList>(0, 0);
            fromTable.Should().BeEquivalentTo(sampleIndexList, options => options.ComparingByMembers<WeightedIndexList>());
        }

        void CheckSame(IHaveReadOnlyTensorSegment<float> tensorSegment1, IHaveReadOnlyTensorSegment<float> tensorSegment2)
        {
            tensorSegment1.ReadOnlySegment.Values.Should().BeEquivalentTo(tensorSegment2.ReadOnlySegment.Values);
        }

        [Fact]
        public void InMemoryVector()
        {
            var builder = _context.CreateTableBuilder();
            var vectorBuilder = builder.AddColumn<IVectorData>("vector");
            var firstVector = _context.CreateReadOnlyVector(5, i => i + 1);
            vectorBuilder.Add(firstVector);
            var secondVector = _context.CreateReadOnlyVector(5, i => i + 2);
            vectorBuilder.Add(secondVector);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IReadOnlyVector>(0, 0).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstVector);
            using var fromTable2 = dataTable.Get<IReadOnlyVector>(1, 0).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable2, secondVector);
        }

        [Fact]
        public void InMemoryMatrix()
        {
            var builder = _context.CreateTableBuilder();
            var matrixBuilder = builder.AddColumn<IMatrixData>("matrix");
            var firstMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j);
            matrixBuilder.Add(firstMatrix);
            var secondMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => (i + j) + 1);
            matrixBuilder.Add(secondMatrix);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            var column = dataTable.GetColumn(0);
            var columnItems = column.AsDataTableSegment<IMatrixData>().Values.ToList();
            columnItems.Last().Should().BeEquivalentTo(secondMatrix);

            using var fromTable = dataTable.Get<IReadOnlyMatrix>(0, 0).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstMatrix);
            var fromTable2 = dataTable.Get<IReadOnlyMatrix>(1, 0);
            CheckSame(fromTable2, secondMatrix);

            dataTable.GetRow(1)[0].Should().BeEquivalentTo(secondMatrix);
        }

        [Fact]
        public void InMemoryTensor3D()
        {
            var builder = _context.CreateTableBuilder();
            var tensorBuilder = builder.AddColumn<ITensor3DData>("tensor");
            var firstTensor = _context.CreateReadOnlyTensor3D(
                _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j),
                _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j)
            );
            tensorBuilder.Add(firstTensor);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IReadOnlyTensor3D>(0, 0).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstTensor);
        }

        [Fact]
        public void InMemoryTensor4D()
        {
            var builder = _context.CreateTableBuilder();
            var tensorBuilder = builder.AddColumn<ITensor4DData>("tensor");
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
            tensorBuilder.Add(firstTensor);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IReadOnlyTensor4D>(0, 0).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstTensor);
        }
    }
}
