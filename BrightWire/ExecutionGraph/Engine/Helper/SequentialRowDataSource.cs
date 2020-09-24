using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single sequence
    /// </summary>
    class SequentialRowDataSource : IDataSource
    {
        readonly float[][] _data;

        public SequentialRowDataSource(float[][] data)
        {
            _data = data;
            InputSize = (uint)data.First().Length;
            InputCount = (uint)data.Length;
        }

        public bool IsSequential => true;
        public uint InputSize { get; }
        public uint? OutputSize { get; } = null;
        public uint RowCount => 1;
        public uint InputCount { get; }

        public IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var ret = new MiniBatch(rows, this);
            int index = 0;
            foreach (var row in _data) {
                var type = MiniBatchSequenceType.Standard;
                if (index == 0)
                    type = MiniBatchSequenceType.SequenceStart;
                else if (index == _data.Length - 1)
                    type = MiniBatchSequenceType.SequenceEnd;
                var inputList = new IGraphData [] {
                    new MatrixGraphData(executionContext.LinearAlgebraProvider.CreateVector(row).ReshapeAsRowMatrix())
                };
                ret.Add(type, inputList, null);
                ++index;
            }
            return ret;
        }

        public uint[][] GetBuckets()
        {
            return new[] {
                new uint [] {
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
