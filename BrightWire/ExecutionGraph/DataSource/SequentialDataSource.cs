﻿using BrightData;
using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataSource
{
    /// <summary>
    /// Feeds sequential data to the graph
    /// </summary>
    class SequentialDataSource : IDataSource
    {
        readonly uint[] _rowDepth;
	    readonly IReadOnlyList<Matrix<float>> _data;
        readonly ILinearAlgebraProvider _lap;

        public SequentialDataSource(ILinearAlgebraProvider lap, IReadOnlyList<Matrix<float>> matrixList)
        {
            _lap = lap;
            _data = matrixList;
            OutputSize = null;

            int index = 0;
            _rowDepth = new uint[matrixList.Count];
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
	    public uint RowCount => (uint)_data.Count;

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<uint> rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();

            var inputData = new Dictionary<uint, List<Vector<float>>>();
            foreach (var item in data) {
                var input = item;
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out var temp))
                        inputData.Add(i, temp = new List<Vector<float>>());
                    temp.Add(FloatVector.Create(input.Row(i).Data));
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = _lap.CreateMatrixFromRows(item.Value);
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                var inputList = new List<IGraphData> {
                    new MatrixGraphData(input)
                };
                miniBatch.Add(type, inputList, null);
            }
            return miniBatch;
        }

        public IReadOnlyList<IReadOnlyList<uint>> GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, (uint)i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => d.Item2).ToList())
                .ToList()
            ;
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
