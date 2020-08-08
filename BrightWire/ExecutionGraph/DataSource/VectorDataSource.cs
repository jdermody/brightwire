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
	    readonly IReadOnlyList<FloatVector> _data;
        readonly ILinearAlgebraProvider _lap;

        public VectorDataSource(ILinearAlgebraProvider lap, IReadOnlyList<FloatVector> data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            InputSize = first.Size;
            OutputSize = -1;
        }

        public int InputCount => 1;
        public bool IsSequential => false;
        public int InputSize { get; }
	    public int OutputSize { get; }
	    public int RowCount => _data.Count;

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = rows.Select(i => _data[i]).ToList();
            var input = _lap.CreateMatrix(data.Count, InputSize, (x, y) => data[x].Data[y]);
            var inputList = new List<IGraphData> {
                new MatrixGraphData(input)
            };
            return new MiniBatch(rows, this, inputList, null);
        }

        public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                Enumerable.Range(0, _data.Count).ToList()
            };
        }

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public void OnBatchProcessed(IContext context)
        {
            // nop
        }
    }
}
