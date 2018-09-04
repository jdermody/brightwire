using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single sequence
    /// </summary>
    class SequentialRowDataSource : IDataSource
    {
        readonly IReadOnlyList<float[]> _data;

	    public SequentialRowDataSource(IReadOnlyList<float[]> data)
        {
            _data = data;
            InputSize = data.First().Length;
            InputCount = data.Count;
        }

        public bool IsSequential => true;
        public int InputSize { get; }
	    public int OutputSize => throw new NotImplementedException();
        public int RowCount => 1;
        public int InputCount { get; }

	    public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var ret = new MiniBatch(rows, this);
            int index = 0;
            foreach(var row in _data) {
                var type = MiniBatchSequenceType.Standard;
                if (index == 0)
                    type = MiniBatchSequenceType.SequenceStart;
                else if (index == _data.Count - 1)
                    type = MiniBatchSequenceType.SequenceEnd;
                var inputList = new [] {
                    new MatrixGraphData(executionContext.LinearAlgebraProvider.CreateVector(row).ReshapeAsRowMatrix())
                };
                ret.Add(type, inputList, null);
            }
            return ret;
        }

        public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                new [] {
                    1
                }
            };
        }

        public void OnBatchProcessed(IContext context)
        {
            // nop
        }
    }
}
