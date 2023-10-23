using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class VectorisationTests : UnitTestBase
    {
        public Task<IDataTable> GetTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Boolean, "bool");
            builder.CreateColumn(BrightDataType.SByte, "byte");
            builder.CreateColumn(BrightDataType.Decimal, "decimal");
            builder.CreateColumn(BrightDataType.Double, "double");
            builder.CreateColumn(BrightDataType.Float, "float");
            builder.CreateColumn(BrightDataType.Int, "int");
            builder.CreateColumn(BrightDataType.Long, "long");
            builder.CreateColumn(BrightDataType.IndexList, "index-list");
            builder.CreateColumn(BrightDataType.WeightedIndexList, "weighted-index-list");
            builder.CreateFixedSizeVectorColumn(3, "vector");

            var indexList = IndexList.Create(1, 2, 3);
            var weightedIndexList = WeightedIndexList.Create((1, 0.1f), (2, 0.5f), (3, 1f));
            var vector = _context.CreateReadOnlyVector(3, 0.25f);
            builder.AddRow(false, (sbyte)1, (decimal)2, (double)3, (float)4, 5, (long)6, indexList, weightedIndexList, vector);
            return builder.BuildInMemory();
        }

        public Task<IDataTable> GetTable2()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String);

            builder.AddRow("a");
            builder.AddRow("b");
            builder.AddRow("c");
            return builder.BuildInMemory();
        }

        [Fact]
        public async Task SimpleVectorisation()
        {
            var table = await GetTable();
            var vectoriser = await table.GetVectoriser(false);
            vectoriser.OutputSize.Should().Be(18);

            var vector = vectoriser.Vectorise(table.GetColumns()).ToBlockingEnumerable().Single();
            vector[0, 0].Should().Be(0);
            vector[1, 0].Should().Be(1);
        }

        [Fact]
        public async Task VectorisationSerialisation()
        {
            var table = await GetTable();
            var vectoriser1 = await table.GetVectoriser(false);
            var vector1 = await vectoriser1.Vectorise(table).ToFloatVectors();

            var vectoriser2 = table.ColumnMetaData.GetVectoriser();
            var vector2 = vectoriser2.Vectorise(table).ToFloatVectors();

            vector1.Should().AllBeEquivalentTo(vector2);
        }

        [Fact]
        public async Task OneHotEncodeSingle()
        {
            var table = await GetTable2();
            var vectoriser = await table.GetVectoriser(false, 0);
            var output = await vectoriser.Vectorise(table).ToFloatVectors();
            output.Count.Should().Be(3);
            output[0].Length.Should().Be(1);
            output[0][0].Should().Be(0);
            output[2][0].Should().Be(2);

            vectoriser.GetOutputLabel(0).Should().Be("a");
            vectoriser.GetOutputLabel(1).Should().Be("b");
            vectoriser.GetOutputLabel(2).Should().Be("c");
        }

        [Fact]
        public async Task OneHotEncodeVector()
        {
            var table = await GetTable2();
            var vectoriser = await table.GetVectoriser(true, 0);
            var output = await vectoriser.Vectorise(table).ToFloatVectors();
            output.Count.Should().Be(3);
            output[0].Length.Should().Be(3);
            output[0][0].Should().Be(1);
            output[2][2].Should().Be(1);

            vectoriser.GetOutputLabel(0).Should().Be("a");
            vectoriser.GetOutputLabel(1).Should().Be("b");
            vectoriser.GetOutputLabel(2).Should().Be("c");
        }

        [Fact]
        public async Task OtherVectorisation()
        {
            var table = await GetTable();
            var row = table[0];
            var rowValues = row.ToArray();
            var vectoriser = await table.GetVectoriser(false);
            var vector1 = await vectoriser.Vectorise(table).ToFloatVectors();
            var vector2 = vectoriser.Vectorise(row);
            var vector3 = vectoriser.Vectorise(rowValues);

            vector1.Should().BeEquivalentTo(vector2);
            vector2.Should().BeEquivalentTo(vector3);
        }
    }
}
