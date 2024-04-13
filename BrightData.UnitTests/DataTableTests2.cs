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
            var dataTable = await _context.LoadTableFromStream(stream);

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
            var dataTable = await _context.LoadTableFromStream(stream);

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
            var dataTable = await _context.LoadTableFromStream(stream);

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
            var vectorBuilder = builder.CreateColumn<ReadOnlyVector<float>>("vector");
            var firstVector = _context.CreateReadOnlyVector(5, i => i + 1);
            vectorBuilder.Append(firstVector);
            var secondVector = _context.CreateReadOnlyVector(5, i => i + 2);
            vectorBuilder.Append(secondVector);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = await _context.LoadTableFromStream(stream);
            var column = dataTable.GetColumn<ReadOnlyVector<float>>(0);
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
            var matrixBuilder = builder.CreateColumn<ReadOnlyMatrix<float>>("matrix");
            var firstMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j);
            matrixBuilder.Append(firstMatrix);
            var secondMatrix = _context.CreateReadOnlyMatrix(5, 5, (i, j) => (i + j) + 1);
            matrixBuilder.Append(secondMatrix);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = await _context.LoadTableFromStream(stream);

            var column = dataTable.GetColumn<ReadOnlyMatrix<float>>(0);
            var columnItems = await column.ToArray();
            columnItems.Last().Should().BeEquivalentTo(secondMatrix);

            using var fromTable = columnItems[0].Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstMatrix);
            var fromTable2 = columnItems[1];
            CheckSame(fromTable2, secondMatrix);

            (await dataTable[1])[0].Should().BeEquivalentTo(secondMatrix);
        }

        [Fact]
        public async Task InMemoryTensor3D()
        {
            var builder = _context.CreateTableBuilder();
            var tensorBuilder = builder.CreateColumn<ReadOnlyTensor3D<float>>("tensor");
            var firstTensor = _context.CreateReadOnlyTensor3D(
                _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j),
                _context.CreateReadOnlyMatrix(5, 5, (i, j) => i + j)
            );
            tensorBuilder.Append(firstTensor);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = await _context.LoadTableFromStream(stream);

            using var fromTable = (await dataTable.Get<ReadOnlyTensor3D<float>>(0, 0)).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstTensor);
        }

        [Fact]
        public async Task InMemoryTensor4D()
        {
            var builder = _context.CreateTableBuilder();
            var tensorBuilder = builder.CreateColumn<ReadOnlyTensor4D<float>>("tensor");
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
            var dataTable = await _context.LoadTableFromStream(stream);

            using var fromTable = (await dataTable.Get<ReadOnlyTensor4D<float>>(0, 0)).Create(_context.LinearAlgebraProvider);
            CheckSame(fromTable, firstTensor);
        }

        Task<IDataTable> CreateSimpleTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn<int>();
            builder.CreateColumn<float>();
            builder.AddRow(1, 2f);
            builder.AddRow(2, 4f);
            return builder.BuildInMemory();
        }

        [Fact]
        public async Task TableRowTests()
        {
            using var table = await CreateSimpleTable();
            var buffer = table.GetRowsBuffer<int, float>();
            var index = 1;
            await foreach (var row in buffer) {
                row.Size.Should().Be(2);
                row.Get<int>(0).Should().Be(1 * index);
                row.Get<float>(1).Should().Be(2f * index);
                ++index;
            }
        }

        [Fact]
        public async Task ObjectConversion()
        {
            using var table = await CreateSimpleTable();
            var buffer = table.GetColumn<object>(0);
            var obj = (await buffer.ToArray())[0];
            obj.Should().BeOfType<int>();
            ((int)obj).Should().Be(1);
        }

        [Fact]
        public async Task StringConversion()
        {
            using var table = await CreateSimpleTable();
            var buffer = table.GetColumn<string>(0);
            var str = (await buffer.ToArray())[0];
            str.Should().BeOfType<string>();
            int.Parse(str).Should().Be(1);
        }

        [Fact]
        public async Task ColumnConversion()
        {
            using var table = await CreateSimpleTable();
            var buffer = table.GetColumn<long>(0);
            var val = (await buffer.ToArray())[0];
            val.Should().Be(1);
        }

        [Fact]
        public async Task BinaryData()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn<BinaryData>();
            var data = new BinaryData(1, 2, 3, 4);
            builder.AddRow(data);
            using var table = await builder.BuildInMemory();
            var data2 = (BinaryData)(await table[0])[0];
            data2.Should().BeEquivalentTo(data);
        }

        [Fact]
        public async Task ColumnMapping()
        {
            using var table = await CreateSimpleTable();
            var mappedColumn = table.GetColumn<int, double>(0, x => x / 2.0);
            var result = await mappedColumn.ToArray();
            result[0].Should().Be(0.5);
        }

        [Fact]
        public async Task GetBlock()
        {
            using var table = await CreateSimpleTable();
            var slice = await table.Get<int>(0, 0, 1);
            slice.Length.Should().Be(2);
        }

        [Fact]
        public async Task PersistMetaData()
        {
            using var table = await CreateSimpleTable();
            table.MetaData.Set("test", 123);
            table.PersistMetaData();
            var buffer = table.GetRowsBuffer<int, float>();
            var index = 1;
            await foreach (var row in buffer) {
                row.Size.Should().Be(2);
                row.Get<int>(0).Should().Be(1 * index);
                row.Get<float>(1).Should().Be(2f * index);
                ++index;
            }

            table.MetaData.Get<int>("test", 0).Should().Be(123);
        }

        [Fact]
        public async Task GetAllColumns()
        {
            using var table = await CreateSimpleTable();
            using var stream = new MemoryStream();
            await table.WriteColumnsTo(stream);
            stream.Position = 0;
            using var table2 = await _context.LoadTableFromStream(stream);
            var matrix2 = await table2.AsMatrix();
            matrix2.Should().BeEquivalentTo(await table.AsMatrix());
        }

        [Fact]
        public async Task GetMany()
        {
            using var table = await CreateSimpleTable();
            var firstRow = await table[0];
            var vals = firstRow.GetMany<string>(0, 1);
            vals[0].Should().Be("1");
            vals[1].Should().Be("2");
        }
    }
}
