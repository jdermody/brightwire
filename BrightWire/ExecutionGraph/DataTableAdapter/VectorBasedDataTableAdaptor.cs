﻿using System;
using System.Linq;
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
    internal class VectorBasedDataTableAdapter(IDataTable dataTable, uint[] featureColumns) : TypedRowBasedDataTableAdapterBase<ReadOnlyVector, ReadOnlyVector>(dataTable, featureColumns)
    {
        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            using var inputRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            using var targetRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            var index = 0;

            await foreach (var row in GetRows(rows)) {
                inputRows.Span[index] = row.C1.ReadOnlySegment;
                targetRows.Span[index] = row.C2.ReadOnlySegment;
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
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns);
        }
    }
}
