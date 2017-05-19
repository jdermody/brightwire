using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.ExecutionGraph.Helper;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    class TensorBasedDataTableAdaptor : AdaptiveDataTableAdaptorBase
    {
        readonly int _inputSize, _outputSize;
        readonly List<(IContext Context, int Rows, int Columns, int Depth)> _processedContext = new List<(IContext, int, int, int)>();

        public TensorBasedDataTableAdaptor(ILearningContext learningContext, IExecutionContext executionContext, GraphFactory factory, IDataTable dataTable, Action<WireBuilder> dataConversionBuilder) 
            : base(learningContext, dataTable, executionContext)
        {
            var firstRow = dataTable.GetRow(0);
            var input = (FloatTensor)firstRow.Data[0];
            var output = (FloatVector)firstRow.Data[1];
            _outputSize = output.Size;

            var wireBuilder = factory.Build(input.Size, _input);
            dataConversionBuilder(wireBuilder);

            // execute the graph to find the input size (which is the size of the adaptive graph's output)
            var firstTensor = new TensorGraphData(_lap.CreateTensor(input));
            var firstContext = _Process(firstTensor);
            var outputVector = firstContext.Data.GetTensor().ConvertToVector();
            _inputSize = outputVector.Count;
            _learningContext.Clear();
        }

        private TensorBasedDataTableAdaptor(ILearningContext learningContext, IExecutionContext executionContext, IDataTable dataTable, INode input, int inputSize, int outputSize) 
            :base(learningContext, dataTable, executionContext)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            _input = input;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new TensorBasedDataTableAdaptor(_learningContext, _executionContext, dataTable, _input, _inputSize, _outputSize);
        }

        public override bool IsSequential => false;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;

        public override IMiniBatch Get(IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows);
            var inputList = new List<IVector>();
            var outputList = new List<FloatVector>();
            _processedContext.Clear();

            var stack = new ConcurrentStack<(IRow, IContext)>();
            Parallel.ForEach(data, row => {
                var tensor = _lap.CreateTensor(row.Data[0] as FloatTensor);
                var context = _ConcurentProcess(new TensorGraphData(tensor, row.Index));
                stack.Push((row, context));
            });
            var contextTable = stack.ToDictionary(d => d.Item1, d => d.Item2);

            foreach (var row in data) {
                //var tensor = _lap.CreateTensor(row.Data[0] as FloatTensor);
                //var context = _Process(new TensorGraphData(tensor));
                var context = contextTable[row];
                var outputTensor = context.Data.GetTensor();
                var outputVector = outputTensor.ConvertToVector();

                inputList.Add(outputVector);
                _processedContext.Add((context, outputTensor.RowCount, outputTensor.ColumnCount, outputTensor.Depth));
                outputList.Add(row.Data[1] as FloatVector);
            }
            var input = _lap.CreateMatrix(inputList);
            var output = _lap.CreateMatrix(data.Count, OutputSize, (x, y) => outputList[x].Data[y]);
            return new MiniBatch(rows, this, input, output);
        }

        public override void OnBatchProcessed(IContext context)
        {
            var errorSignal = context.ErrorSignal;
            var lap = context.LinearAlgebraProvider;
            if (errorSignal != null) {
                var errorMatrix = errorSignal.GetMatrix();
                for (var i = 0; i < errorMatrix.RowCount; i++) {
                    var row = errorMatrix.Row(i);
                    var processedContext = _processedContext[i];
                    var rowErrorTensor = row.ConvertToTensor(lap, processedContext.Rows, processedContext.Columns, processedContext.Depth);
                    processedContext.Item1.Backpropagate(new TensorGraphData(rowErrorTensor));
                }
            }

            foreach (var item in _processedContext) {
                item.Item1.LearningContext.ApplyUpdates();
            }
            _processedContext.Clear();
        }
    }
}
