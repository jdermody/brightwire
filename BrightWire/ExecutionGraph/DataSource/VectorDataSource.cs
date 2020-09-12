using BrightData;
using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataSource
{
    /// <summary>
    /// Feeds data to the execution graph
    /// </summary>
    class VectorDataSource : IDataSource
    {
	    readonly IReadOnlyList<Vector<float>> _data;
        readonly ILinearAlgebraProvider _lap;

        public VectorDataSource(ILinearAlgebraProvider lap, IReadOnlyList<Vector<float>> data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            InputSize = first.Size;
            OutputSize = null;
        }

        public uint InputCount => 1;
        public bool IsSequential => false;
        public uint InputSize { get; }
	    public uint? OutputSize { get; }
	    public uint RowCount => (uint)_data.Count;

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<uint> rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();
            var input = _lap.CreateMatrix((uint)data.Count, (uint)InputSize, (x, y) => data[(int)x].Segment[y]);
            var inputList = new List<IGraphData> {
                new MatrixGraphData(input)
            };
            return new MiniBatch(rows, this, inputList, null);
        }

        public IReadOnlyList<IReadOnlyList<uint>> GetBuckets()
        {
            return new[] {
                _data.Count.AsRange().ToList()
            };
        }

        public IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public void OnBatchProcessed(IContext context)
        {
            // nop
        }
    }
}
