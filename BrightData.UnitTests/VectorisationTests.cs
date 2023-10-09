﻿using System.IO;
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

            var indexList = _context.CreateIndexList(1, 2, 3);
            var weightedIndexList = _context.CreateWeightedIndexList((1, 0.1f), (2, 0.5f), (3, 1f));
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
        public void SimpleVectorisation()
        {
            var table = GetTable();
            var vectoriser = table.GetVectoriser(false);
            vectoriser.OutputSize.Should().Be(18);

            var vector = vectoriser.Enumerate().Single();
            vector[0].Should().Be(0);
            vector[1].Should().Be(1);
        }

        [Fact]
        public void VectorisationSerialisation()
        {
            var table = GetTable();
            var vectoriser1 = table.GetVectoriser(false);
            var vector1 = vectoriser1.Enumerate().Single();

            var reader = new BinaryReader(new MemoryStream(vectoriser1.GetData()), Encoding.UTF8);
            var vectoriser2 = table.LoadVectoriser(reader);
            var vector2 = vectoriser2.Enumerate().Single();

            vector1.Should().BeEquivalentTo(vector2);
        }

        [Fact]
        public void OneHotEncodeSingle()
        {
            var table = GetTable2();
            var vectoriser = table.GetVectoriser(false, 0);
            var output = vectoriser.Enumerate().ToList();
            output.Count.Should().Be(3);
            output[0].Size.Should().Be(1);
            output[0][0].Should().Be(0);
            output[2][0].Should().Be(2);

            vectoriser.GetOutputLabel(0).Should().Be("a");
            vectoriser.GetOutputLabel(1).Should().Be("b");
            vectoriser.GetOutputLabel(2).Should().Be("c");
        }

        [Fact]
        public void OneHotEncodeVector()
        {
            var table = GetTable2();
            var vectoriser = table.GetVectoriser(true, 0);
            var output = vectoriser.Enumerate().ToList();
            output.Count.Should().Be(3);
            output[0].Size.Should().Be(3);
            output[0][0].Should().Be(1);
            output[2][2].Should().Be(1);

            vectoriser.GetOutputLabel(0).Should().Be("a");
            vectoriser.GetOutputLabel(1).Should().Be("b");
            vectoriser.GetOutputLabel(2).Should().Be("c");
        }

        [Fact]
        public void OtherVectorisation()
        {
            var table = GetTable();
            var row = table.GetRow(0);
            var rowValues = row.ToArray();
            var vectoriser = table.GetVectoriser(false);
            var vector1 = vectoriser.Enumerate().Single().Segment.ToNewArray();
            var vector2 = vectoriser.Vectorise(row);
            var vector3 = vectoriser.Vectorise(rowValues);

            vector1.Should().BeEquivalentTo(vector2);
            vector2.Should().BeEquivalentTo(vector3);
        }
    }
}
