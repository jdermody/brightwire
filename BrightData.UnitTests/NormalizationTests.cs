using BrightData.UnitTests.Helper;
using System.Linq;
using BrightData.Helper;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;

namespace BrightData.UnitTests
{
    public class NormalizationTests : UnitTestBase
    {
        Task<IDataTable> GetTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Double);
            builder.CreateColumn(BrightDataType.Double);
            builder.AddRow(100d, 200d);
            builder.AddRow(200d, 300d);
            builder.AddRow(-50d, -100d);
            return builder.BuildInMemory();
        }

        static void ValidateNormalization(IDataTable normalized, IDataTable original)
        {
            var normalization = normalized.ColumnMetaData.Select(x => x.GetNormalization()).ToArray();

            foreach(var (normalizedRow, originalRow) in normalized.EnumerateRows().ToBlockingEnumerable().Zip(original.EnumerateRows().ToBlockingEnumerable())) {
                for (uint i = 0; i < original.ColumnCount; i++) {
                    var originalValue = originalRow.Get<double>(i);
                    var normalizedValue = normalizedRow.Get<double>(i);
                    normalizedValue.Should().BeInRange(-1.5, 1.5);
                    var reverseNormalized = normalization[i].ReverseNormalize(normalizedValue);
                    DoubleMath.AreApproximatelyEqual(reverseNormalized, originalValue, 0.1).Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task EuclideanNormalization()
        {
            using var table = await GetTable();
            using var normalized = await table.Normalize(NormalizationType.Euclidean);
            ValidateNormalization(normalized, table);
        }

        [Fact]
        public async Task FeatureScaleNormalization()
        {
            using var table = await GetTable();
            using var normalized = await table.Normalize(NormalizationType.FeatureScale);
            ValidateNormalization(normalized, table);
        }

        [Fact]
        public async Task ManhattanNormalization()
        {
            using var table = await GetTable();
            using var normalized = await table.Normalize(NormalizationType.Manhattan);
            ValidateNormalization(normalized, table);
        }

        [Fact]
        public async Task StandardNormalization()
        {
            using var table = await GetTable();
            using var normalized = await table.Normalize(NormalizationType.Standard);
            ValidateNormalization(normalized, table);
        }
    }
}
