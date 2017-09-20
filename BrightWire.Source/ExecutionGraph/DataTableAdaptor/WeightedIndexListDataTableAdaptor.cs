using System;
using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables with a weighted index list based column (corresponding to a sparse vector)
    /// </summary>
    class WeightedIndexListDataTableAdaptor : DataTableAdaptorBase<(List<WeightedIndexList>, FloatVector)>, IWeightedIndexListEncoder
    {
        readonly int _inputSize, _outputSize;
        readonly IDataTableVectoriser _vectoriser;

        public WeightedIndexListDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable, IDataTableVectoriser vectoriser)
            : base(lap, dataTable)
        {
            _vectoriser = vectoriser;
            _inputSize = vectoriser.InputSize;
            _outputSize = _vectoriser.OutputSize;

            // load the data
            dataTable.ForEach(row => _data.Add((_dataColumnIndex.Select(i => row.GetField<WeightedIndexList>(i)).ToList(), _vectoriser.GetOutput(row))));
        }

        public override bool IsSequential => false;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;
        public override int RowCount => _data.Count;

        public float[] Encode(WeightedIndexList indexList)
        {
            var ret = new float[_inputSize];
            foreach (var item in indexList.IndexList)
                ret[item.Index] = item.Weight;
            return ret;
        }

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (r.Item1.Select(d => Encode(d)).ToArray(), r.Item2.Data))
                .ToList()
            ;
            return _GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new WeightedIndexListDataTableAdaptor(_lap, dataTable, _vectoriser);
        }
    }
}
