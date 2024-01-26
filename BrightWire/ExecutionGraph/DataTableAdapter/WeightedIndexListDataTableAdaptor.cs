﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.DataTable;
using BrightData.Types;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables with weighted index list based columns (corresponding to a sparse vector)
    /// </summary>
    internal class WeightedIndexListDataTableAdapter : DataTableAdapterBase<(WeightedIndexList IndexList, float[] Output)>, IWeightedIndexListEncoder
    {
        readonly uint[] _featureColumns;

        public WeightedIndexListDataTableAdapter(IDataTable dataTable, VectorisationModel? outputVectoriser, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            OutputVectoriser = outputVectoriser ?? dataTable.GetVectoriser(true, dataTable.GetTargetColumnOrThrow()).Result;
            OutputSize = OutputVectoriser.OutputSize;

            var analysis = dataTable.GetColumnAnalysis(_featureColumnIndices).Result.Select(m => m.GetIndexAnalysis());
            InputSize = analysis.Max(a => a.MaxIndex ?? throw new ArgumentException("Could not find the max index")) + 1;
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }

        protected override async IAsyncEnumerable<(WeightedIndexList IndexList, float[] Output)> GetRows(uint[] rows)
        {
            var data = await _dataTable.GetRows(rows);
            foreach(var tableRow in data)
                yield return (Combine(_featureColumnIndices.Select(i => (WeightedIndexList)tableRow[i])), OutputVectoriser!.Vectorise(tableRow));
        }

        public WeightedIndexList Combine(IEnumerable<WeightedIndexList> lists) => WeightedIndexList.Merge(lists);

        public float[] Encode(WeightedIndexList indexList)
        {
            var ret = new float[InputSize];
            foreach (ref readonly var item in indexList.ReadOnlySpan)
                ret[item.Index] = item.Weight;
            return ret;
        }

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var index = 0;
            var data = new (float[], float[])[rows.Length];
            await foreach (var row in GetRows(rows))
                data[index++] = (Encode(row.IndexList), row.Output);
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new WeightedIndexListDataTableAdapter(dataTable, OutputVectoriser, _featureColumns);
        }
    }
}
