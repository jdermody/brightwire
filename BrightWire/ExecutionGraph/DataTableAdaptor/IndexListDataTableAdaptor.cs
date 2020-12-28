using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightTable;
using BrightTable.Transformations;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables with a index list based column (corresponding to an unweighted sparse vector)
    /// </summary>
    class IndexListDataTableAdaptor : DataTableAdaptorBase<(IndexList, float[])>, IIndexListEncoder
    {
        private readonly uint[] _featureColumns;

        public IndexListDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, IVectorise outputVectoriser, uint[] featureColumns)
            : base(lap, dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            OutputVectoriser = outputVectoriser ?? new DataTableVectoriser(dataTable, dataTable.GetTargetColumnOrThrow());
            OutputSize = OutputVectoriser.OutputSize;

            // load the data
            uint inputSize = 0;
            dataTable.ForEachRow(row => _data.Add((Combine(_dataColumnIndex.Select(i => (IndexList)row[i]), ref inputSize), OutputVectoriser.Vectorise(row))));
            InputSize = inputSize;
        }

        public override bool IsSequential => false;
        public override uint InputSize { get; }
        public override uint? OutputSize { get; }
        public override uint RowCount => (uint)_data.Count;

        public IndexList Combine(IEnumerable<IndexList> lists, ref uint inputSize)
        {
            var ret = IndexList.Merge(lists);
            var maxIndex = ret.Indices.Max();
            if (maxIndex > inputSize)
                inputSize = maxIndex + 1;
            return ret;
        }

        public float[] Encode(IndexList indexList)
        {
            var ret = new float[InputSize];
            foreach (var group in indexList.Indices.GroupBy(d => d))
                ret[group.Key] = group.Count();
            return ret;
        }

        public override IMiniBatch Get(IGraphExecutionContext executionContext, uint[] rows)
        {
            var data = _GetRows(rows).Select(r => (new[] { Encode(r.Item1)}, r.Item2)).ToArray();
            return _GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new IndexListDataTableAdaptor(_lap, dataTable, OutputVectoriser, _featureColumns);
        }
    }
}
