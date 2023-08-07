using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
using BrightWire.ExecutionGraph;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
	public class DataSourceTests : CpuBase
	{
        readonly GraphFactory _factory;

		public DataSourceTests()
		{
            _factory = new GraphFactory(_cpu);
		}

        [Fact]
		public void DefaultDataSource()
		{
			var builder = _context.CreateTableBuilder();
			builder.AddColumn(BrightDataType.Float, "val1");
			builder.AddColumn(BrightDataType.Double, "val2");
			builder.AddColumn(BrightDataType.String, "val3");
			builder.AddColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

			builder.AddRow(0.5f, 1.1, "d", "a");
			builder.AddRow(0.2f, 1.5, "c", "b");
			builder.AddRow(0.7f, 0.5, "b", "c");
			builder.AddRow(0.2f, 0.6, "a", "d");

			var table = builder.BuildInMemory();
            var dataSource = _factory.CreateDataSource(table);
			var miniBatch = dataSource.Get(new uint[] { 1 });
			var input = miniBatch.CurrentSequence.Input!.GetMatrix().GetRowAsReadOnly(0);
			var expectedOutput = miniBatch.CurrentSequence.Target!.GetMatrix().GetRowAsReadOnly(0);

            input[0].Should().Be(0.2f);
            input[1].Should().Be(1.5f);
            expectedOutput.Size.Should().Be(4);
            dataSource.OutputVectoriser!.GetOutputLabel(expectedOutput.ReadOnlySegment.GetMinAndMaxValues().MaxIndex).Should().Be("b");
		}

        static float[] GetArray(uint value, uint size)
		{
			var ret = new float[size];
			for (var i = 0; i < size; i++)
				ret[i] = value;
			return ret;
		}

		[Fact]
		public void VectorDataSource()
		{
			var vectors = 10.AsRange().Select(i => _cpu.CreateVector(GetArray(i, 10))).ToArray();
			var dataSource = _factory.CreateDataSource(vectors);
			var miniBatch = dataSource.Get(new uint[] { 0, 1, 2 });

			var currentSequence = miniBatch.CurrentSequence;
			var batchMatrix = currentSequence.Input!.GetMatrix();
            currentSequence.Target.Should().BeNull();
            batchMatrix.RowCount.Should().Be(3);
            batchMatrix.ColumnCount.Should().Be(10);
            batchMatrix.GetRowAsReadOnly(0)[0].Should().Be(0f);
            batchMatrix.GetRowAsReadOnly(1)[0].Should().Be(1f);
        }

		[Fact]
		public void MatrixDataSource()
		{
			var matrices = Enumerable.Range(0, 10).Select(_ => _cpu.CreateMatrixFromRows(10.AsRange().Select(i => _cpu.CreateVector(GetArray(i, 10))).ToArray())).ToArray();
			var dataSource = _factory.CreateDataSource(matrices);
			var miniBatch = dataSource.Get(new uint[] { 0, 1, 2 });

			var currentSequence = miniBatch.CurrentSequence;
			var batchMatrix = currentSequence.Input!.GetMatrix();
            currentSequence.Target.Should().BeNull();
            batchMatrix.RowCount.Should().Be(3);
            batchMatrix.ColumnCount.Should().Be(10);
            batchMatrix.GetRowAsReadOnly(0)[0].Should().Be(0f);
        }

		[Fact]
		public void TensorDataSource()
		{
			var tensors = Enumerable.Range(0, 10).Select(_ => _cpu.CreateTensor3DAndThenDisposeInput(10.AsRange().Select(_ => _cpu.CreateMatrixFromRows(10.AsRange().Select(i => _cpu.CreateVector(GetArray(i, 10))).ToArray())).ToArray())).ToArray();
			var dataSource = _factory.CreateDataSource(tensors);
			var miniBatch = dataSource.Get(new uint[] { 0, 1, 2 });

			var currentSequence = miniBatch.CurrentSequence;
			var batchMatrix = currentSequence.Input!.GetMatrix();

            currentSequence.Target.Should().BeNull();
            batchMatrix.RowCount.Should().Be(1000);
            batchMatrix.ColumnCount.Should().Be(3);
            batchMatrix.GetRowAsReadOnly(0)[0].Should().Be(0f);
        }
	}
}
