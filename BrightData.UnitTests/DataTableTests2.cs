using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable;
using FluentAssertions;
using Xunit;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightData.UnitTests
{
    public class DataTableTests2
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public void InMemoryColumnOriented()
        {
            using var builder = new BrightDataTableBuilder(_context);
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
            using var builder = new BrightDataTableBuilder(_context);
            var indexListBuilder = builder.AddColumn<IndexList>("index list");
            var sampleIndexList = _context.CreateIndexList(1, 2, 3, 4, 5);
            indexListBuilder.Add(sampleIndexList);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            var fromTable = dataTable.Get<IndexList>(0, 0);
            fromTable.Should().BeEquivalentTo(sampleIndexList);
        }

        [Fact]
        public void InMemoryWeightedIndexList()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var indexListBuilder = builder.AddColumn<WeightedIndexList>("weighted index list");
            var sampleIndexList = _context.CreateWeightedIndexList((1, 1f), (2, 0.1f), (3, 0.75f), (4, 0.25f), (5, 0.77f));
            indexListBuilder.Add(sampleIndexList);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            var fromTable = dataTable.Get<WeightedIndexList>(0, 0);
            fromTable.Should().BeEquivalentTo(sampleIndexList);
        }

        [Fact]
        public void InMemoryVector()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var vectorBuilder = builder.AddColumn<IReadOnlyVector>("vector");
            using var firstVector = _context.LinearAlgebraProvider.CreateVector(5, i => i + 1);
            vectorBuilder.Add(firstVector);
            using var secondVector = _context.LinearAlgebraProvider.CreateVector(5, i => i + 2);
            vectorBuilder.Add(secondVector);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IReadOnlyVector>(0, 0).Create(_context.LinearAlgebraProvider);
            fromTable.Should().BeEquivalentTo(firstVector);
            using var fromTable2 = dataTable.Get<IReadOnlyVector>(1, 0).Create(_context.LinearAlgebraProvider);
            fromTable2.Should().BeEquivalentTo(secondVector);
        }

        [Fact]
        public void InMemoryMatrix()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var matrixBuilder = builder.AddColumn<IReadOnlyMatrix>("matrix");
            using var firstMatrix = _context.LinearAlgebraProvider.CreateMatrix(5, 5, (i, j) => i + j);
            matrixBuilder.Add(firstMatrix);
            var secondMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => (i + j) + 1);
            matrixBuilder.Add(secondMatrix);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            var column = dataTable.GetColumn(0);
            var columnItems = column.AsDataTableSegment<IReadOnlyMatrix>().Values.ToList();
            columnItems.Last().Should().BeEquivalentTo(secondMatrix);

            using var fromTable = dataTable.Get<IReadOnlyMatrix>(0, 0).Create(_context.LinearAlgebraProvider);
            fromTable.Should().BeEquivalentTo(firstMatrix);
            var fromTable2 = dataTable.Get<IReadOnlyMatrix>(1, 0);
            fromTable2.Should().BeEquivalentTo(secondMatrix);

            dataTable.GetRow(1)[0].Should().BeEquivalentTo(secondMatrix);
        }

        [Fact]
        public void InMemoryTensor3D()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var tensorBuilder = builder.AddColumn<IReadOnlyTensor3D>("tensor");
            var lap = _context.LinearAlgebraProvider;
            using var firstTensor = lap.CreateTensor3DAndThenDisposeInput(
                lap.CreateMatrix(5, 5, (i, j) => i + j),
                lap.CreateMatrix(5, 5, (i, j) => i + j)
            );
            tensorBuilder.Add(firstTensor);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IReadOnlyTensor3D>(0, 0).Create(_context.LinearAlgebraProvider);
            fromTable.Should().BeEquivalentTo(firstTensor);
            fromTable.Segment.Should().BeEquivalentTo(firstTensor.Segment);
        }

        [Fact]
        public void InMemoryTensor4D()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var tensorBuilder = builder.AddColumn<IReadOnlyTensor4D>("tensor");
            var lap = _context.LinearAlgebraProvider;
            using var firstTensor = lap.CreateTensor4DAndThenDisposeInput(
                lap.CreateTensor3DAndThenDisposeInput(
                    lap.CreateMatrix(5, 5, (i, j) => i + j),
                    lap.CreateMatrix(5, 5, (i, j) => i + j)
                ),
                lap.CreateTensor3DAndThenDisposeInput(
                    lap.CreateMatrix(5, 5, (i, j) => i + j),
                    lap.CreateMatrix(5, 5, (i, j) => i + j)
                )
            );
            tensorBuilder.Add(firstTensor);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IReadOnlyTensor4D>(0, 0).Create(_context.LinearAlgebraProvider);
            fromTable.Should().BeEquivalentTo(firstTensor);
            fromTable.Segment.Should().BeEquivalentTo(firstTensor.Segment);
        }
    }
}
