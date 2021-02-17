using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables with weighted index list based columns (corresponding to a sparse vector)
    /// </summary>
    internal class WeightedIndexListDataTableAdapter : DataTableAdapterBase<(WeightedIndexList, float[])>, IWeightedIndexListEncoder
    {
        readonly uint[] _featureColumns;

        public WeightedIndexListDataTableAdapter(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, IDataTableVectoriser? outputVectoriser, uint[] featureColumns)
            : base(lap, dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            OutputVectoriser = outputVectoriser ?? dataTable.GetVectoriser(true, dataTable.GetTargetColumnOrThrow());
            OutputSize = OutputVectoriser.OutputSize;

            // load the data
            uint inputSize = 0;
            dataTable.ForEachRow(row => _data.Add((Combine(_featureColumnIndices.Select(i => (WeightedIndexList)row[i]), ref inputSize), OutputVectoriser.Vectorise(row))));
            InputSize = inputSize;
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }
        public override uint RowCount => (uint)_data.Count;

        public WeightedIndexList Combine(IEnumerable<WeightedIndexList> lists, ref uint inputSize)
        {
            var ret = WeightedIndexList.Merge(lists);
            var maxIndex = ret.Indices.Max(i => i.Index);
            if (maxIndex > inputSize)
                inputSize = maxIndex + 1;
            return ret;
        }

        public float[] Encode(WeightedIndexList indexList)
        {
            var ret = new float[InputSize];
            foreach (var item in indexList.Indices)
                ret[item.Index] = item.Weight;
            return ret;
        }

        public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (new[] { Encode(r.Item1) }, r.Item2))
                .ToArray()
            ;
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new WeightedIndexListDataTableAdapter(_lap, dataTable, OutputVectoriser, _featureColumns);
        }
    }
}
