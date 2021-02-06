using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Executes a preliminary graph and uses its output as the input for the main graph
    /// </summary>
    internal class SequenceToSequenceDataTableAdaptor : AdaptiveDataTableAdaptorBase
    {
        readonly uint[] _rowDepth;
        readonly uint _inputSize, _outputSize;

        public SequenceToSequenceDataTableAdaptor(
            ILinearAlgebraProvider lap, 
            ILearningContext? learningContext, 
            GraphFactory factory, 
            IRowOrientedDataTable dataTable, 
            Action<WireBuilder> dataConversionBuilder
        )
            : this(lap, learningContext, dataTable)
        {
            var wireBuilder = factory.Connect(_inputSize, _input);
            dataConversionBuilder(wireBuilder);

            // execute the graph to find the input size (which is the size of the adaptive graph's output)
            using var executionContext = new ExecutionContext(lap);
            var output = Encode(executionContext, new uint [] { 0 });
            _inputSize = output.Item1.ColumnCount;
            _learningContext?.Clear();
        }

        public SequenceToSequenceDataTableAdaptor(
            ILinearAlgebraProvider lap, 
            ILearningContext? learningContext, 
            IRowOrientedDataTable dataTable, 
            INode input, 
            DataSourceModel dataSource
        )
            : this(lap, learningContext, dataTable)
        {
            _input = input;
            _inputSize = dataSource.InputSize;
            _outputSize = dataSource.OutputSize;
        }

        SequenceToSequenceDataTableAdaptor(
            ILinearAlgebraProvider lap, 
            ILearningContext? learningContext, 
            IRowOrientedDataTable dataTable, 
            INode input, 
            uint inputSize, 
            uint outputSize
        )
            : this(lap, learningContext, dataTable)
        {
            _input = input;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        SequenceToSequenceDataTableAdaptor(
            ILinearAlgebraProvider lap, 
            ILearningContext? learningContext, 
            IRowOrientedDataTable dataTable
        ) 
            : base(lap, learningContext, dataTable)
        {
            _rowDepth = new uint[dataTable.RowCount];
            Matrix<float>? inputMatrix = null, outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputMatrix = (Matrix<float>)row[0];
                outputMatrix = (Matrix<float>)row[1];
                _rowDepth[i] = outputMatrix.RowCount;
            });
            if (inputMatrix == null || outputMatrix == null)
                throw new ArgumentException("No matrices found in data table");

            _inputSize = inputMatrix.ColumnCount;
            _outputSize = outputMatrix.ColumnCount;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new SequenceToSequenceDataTableAdaptor(_lap, _learningContext, dataTable, _input, _inputSize, _outputSize);
        }

        public override bool IsSequential => true;
        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;

        public override uint[][] GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (Row: r, Index: i))
                .GroupBy(t => t.Row)
                .Select(g => g.Select(d => (uint)d.Index).ToArray())
                .ToArray()
            ;
        }

        (IFloatMatrix, object[][]) Encode(IGraphExecutionContext executionContext, uint[] rows)
        {
            var data = GetRows(rows).ToArray();

            // create the input batch
            var inputData = data.Select(r => ((Matrix<float>) r[0], (Matrix<float>?) null)).ToArray();
            var encoderInput = GetSequentialMiniBatch(rows, inputData);

            // execute the encoder
            IMiniBatchSequence? sequence;
            IFloatMatrix? encoderOutput = null;
            while ((sequence = encoderInput.GetNextSequence()) != null) {
                using var context = Process(executionContext, sequence);
                if (sequence.Type == MiniBatchSequenceType.SequenceEnd)
                    encoderOutput = context.Data.GetMatrix();
            }

            if (encoderOutput == null)
                throw new Exception("Encoder produced no output");
            return (encoderOutput, data);
        }

        public override IMiniBatch Get(uint[] rows) => throw new NotImplementedException();

        public override IMiniBatch Get(IGraphExecutionContext executionContext, uint[] rows)
        {
            var (encoderOutput, data) = Encode(executionContext, rows);

            // create the decoder input
            var outputData = new Dictionary<uint, List<Vector<float>>>();
            foreach (var item in data) {
                var output = (Matrix<float>)item[1];
                for (uint i = 0, len = output.RowCount; i < len; i++) {
                    if (!outputData.TryGetValue(i, out var temp))
                        outputData.Add(i, temp = new List<Vector<float>>());
                    temp.Add(output.Row(i));
                }
            }
            var miniBatch = new MiniBatch(rows, this);
            var curr = encoderOutput;
            foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                var output = _lap.CreateMatrixFromRows(item.Value);
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (outputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                var inputList = new IGraphData[] {
                    new MatrixGraphData(curr)
                };
                miniBatch.Add(type, inputList, new MatrixGraphData(output));
                curr = output;
            }
            return miniBatch;
        }

        public override void OnBatchProcessed(IGraphContext context)
        {
            var batch = context.BatchSequence;
            if(context.LearningContext != null && batch.Type == MiniBatchSequenceType.SequenceStart) {
                context.LearningContext.DeferBackpropagation(null, signal => {
                    context.LearningContext.BackpropagateThroughTime(signal);
                });
            }
        }
    }
}
