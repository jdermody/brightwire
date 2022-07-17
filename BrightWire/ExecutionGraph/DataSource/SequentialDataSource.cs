using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.LinearAlgebra;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataSource
{
    /// <summary>
    /// Feeds sequential data to the graph
    /// </summary>
    internal class SequentialDataSource : IDataSource
    {
        readonly uint[] _rowDepth;
	    readonly IMatrix[] _data;
        readonly LinearAlgebraProvider _lap;

        public SequentialDataSource(LinearAlgebraProvider lap, IMatrix[] matrixList)
        {
            _lap = lap;
            _data = matrixList;
            OutputSize = null;

            int index = 0;
            _rowDepth = new uint[matrixList.Length];
            foreach (var item in matrixList) {
                if(index == 0)
                    InputSize = item.ColumnCount;
                _rowDepth[index++] = item.RowCount;
            }
        }

        public uint InputCount => 1;
        public bool IsSequential => true;
        public uint InputSize { get; }
	    public uint? OutputSize { get; }
	    public uint RowCount => (uint)_data.Length;

        public IMiniBatch Get(uint[] rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();

            var inputData = new Dictionary<uint, List<IReadOnlyVector>>();
            foreach (var item in data) {
                var input = item;
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out var temp))
                        inputData.Add(i, temp = new());
                    temp.Add(input.GetRow(i));
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = _lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, input.AsGraphData(), null);
            }
            return miniBatch;
        }

        public uint[][] GetSequentialBatches()
        {
            return _rowDepth
                .Select((r, i) => (Row: r, Index: (uint)i))
                .GroupBy(t => t.Row)
                .Select(g => g.Select(d => d.Index).ToArray())
                .ToArray()
            ;
        }

        public IDataSource CloneWith(BrightDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IDataTableVectoriser? InputVectoriser { get; } = null;
        public IDataTableVectoriser? OutputVectoriser { get; } = null;
    }
}
