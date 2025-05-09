﻿using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra.ReadOnly;
using BrightWire.ExecutionGraph.Helper;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Segment table adapter for tables with vector data
    /// </summary>
    internal class VectorBasedDataTableAdapter : TypedRowBasedDataTableAdapterBase<ReadOnlyVector<float>, ReadOnlyVector<float>>
    {
        /// <summary>
        /// Segment table adapter for tables with vector data
        /// </summary>
        VectorBasedDataTableAdapter(IDataTable dataTable, uint[] featureColumns, uint inputSize, uint? outputSize) : base(dataTable, featureColumns)
        {
            InputSize = inputSize;
            OutputSize = outputSize;
        }

        public static async Task<VectorBasedDataTableAdapter> Create(IDataTable dataTable, uint[] featureColumns)
        {
            var targetColumn = dataTable.GetTargetColumn();
            ReadOnlyVector<float> firstVector;
            var outputSize = 0U;

            if (targetColumn.HasValue) {
                var firstRow = await dataTable.GetRow<ReadOnlyVector<float>, ReadOnlyVector<float>>(0, featureColumns.Single(), targetColumn.Value);
                firstVector = firstRow.Column1;
                outputSize = firstRow.Column2.Size;
            }
            else {
                firstVector = await dataTable.Get<ReadOnlyVector<float>>(featureColumns.Single(), 0);
            }
            return new(dataTable, featureColumns, firstVector.Size, outputSize);
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            using var inputRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            using var targetRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            var index = 0;

            await foreach (var row in GetRows(rows)) {
                inputRows.Span[index] = row.Column1.ReadOnlySegment;
                targetRows.Span[index] = row.Column2.ReadOnlySegment;
                ++index;
            }

            var input = lap.CreateMatrixFromRows(inputRows.Span);
            var output = OutputSize > 0
                ? lap.CreateMatrixFromRows(targetRows.Span)
                : null;

            return new MiniBatch(rows, this, input.AsGraphData(), output?.AsGraphData());
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns, InputSize, OutputSize);
        }
    }
}
