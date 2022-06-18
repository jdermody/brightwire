using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable2;
using BrightData.LinearAlegbra2;
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
            stringReader.EnumerateTyped().Count().Should().Be(2);
            stringReader.EnumerateTyped().Count().Should().Be(2);

            using var intReader = dataTable.ReadColumn<int>(1);
            intReader.EnumerateTyped().Count().Should().Be(2);
            intReader.EnumerateTyped().Count().Should().Be(2);

            var str = dataTable.Get<string>(0, 0);
            var str2 = dataTable.Get<string>(1, 0);
            var val1 = dataTable.Get<int>(0, 1);
            var val2 = dataTable.Get<int>(1, 1);

            using var row1 = dataTable.GetRow(0);
            using var row2 = dataTable.GetRow(1);
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
            var vectorBuilder = builder.AddColumn<IVector>("vector");
            using var firstVector = _context.LinearAlgebraProvider2.CreateVector(5, i => i + 1);
            vectorBuilder.Add(firstVector);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IVector>(0, 0);
            fromTable.Should().BeEquivalentTo(firstVector);
            fromTable.Segment.Should().BeEquivalentTo(firstVector.Segment);
        }

        [Fact]
        public void InMemoryMatrix()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var matrixBuilder = builder.AddColumn<IMatrix>("matrix");
            using var firstMatrix = _context.LinearAlgebraProvider2.CreateMatrix(5, 5, (i, j) => i + j);
            matrixBuilder.Add(firstMatrix);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<IMatrix>(0, 0);
            fromTable.Should().BeEquivalentTo(firstMatrix);
            fromTable.Segment.Should().BeEquivalentTo(firstMatrix.Segment);
        }

        [Fact]
        public void InMemoryTensor3D()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var tensorBuilder = builder.AddColumn<ITensor3D>("tensor");
            var lap = _context.LinearAlgebraProvider2;
            using var firstTensor = lap.CreateTensor3D(true,
                lap.CreateMatrix(5, 5, (i, j) => i + j),
                lap.CreateMatrix(5, 5, (i, j) => i + j)
            );
            tensorBuilder.Add(firstTensor);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<ITensor3D>(0, 0);
            fromTable.Should().BeEquivalentTo(firstTensor);
            fromTable.Segment.Should().BeEquivalentTo(firstTensor.Segment);
        }

        [Fact]
        public void InMemoryTensor4D()
        {
            using var builder = new BrightDataTableBuilder(_context);
            var tensorBuilder = builder.AddColumn<ITensor4D>("tensor");
            var lap = _context.LinearAlgebraProvider2;
            using var firstTensor = lap.CreateTensor4D(true,
                lap.CreateTensor3D(true,
                    lap.CreateMatrix(5, 5, (i, j) => i + j),
                    lap.CreateMatrix(5, 5, (i, j) => i + j)
                ),
                lap.CreateTensor3D(true,
                    lap.CreateMatrix(5, 5, (i, j) => i + j),
                    lap.CreateMatrix(5, 5, (i, j) => i + j)
                )
            );
            tensorBuilder.Add(firstTensor);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var fromTable = dataTable.Get<ITensor4D>(0, 0);
            fromTable.Should().BeEquivalentTo(firstTensor);
            fromTable.Segment.Should().BeEquivalentTo(firstTensor.Segment);
        }
    }
}
