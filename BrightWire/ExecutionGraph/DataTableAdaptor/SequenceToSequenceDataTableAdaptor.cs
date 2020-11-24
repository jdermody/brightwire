using BrightTable;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Executes a preliminary graph and uses its output as the input for the main graph
    /// </summary>
    class SequenceToSequenceDataTableAdaptor : AdaptiveDataTableAdaptorBase
    {
        uint[] _rowDepth;
        uint _inputSize, _outputSize;

        public SequenceToSequenceDataTableAdaptor(ILinearAlgebraProvider lap, ILearningContext learningContext, GraphFactory factory, IRowOrientedDataTable dataTable, Action<WireBuilder> dataConversionBuilder)
            : base(lap, learningContext, dataTable)
        {
            _Initialise(dataTable);

            var wireBuilder = factory.Connect(_inputSize, _input);
            dataConversionBuilder(wireBuilder);

            // execute the graph to find the input size (which is the size of the adaptive graph's output)
            using var executionContext = new ExecutionContext(lap);
            var output = _Encode(executionContext, new uint [] { 0 });
            _inputSize = output.Item1.ColumnCount;
            _learningContext.Clear();
        }

        public SequenceToSequenceDataTableAdaptor(ILinearAlgebraProvider lap, ILearningContext learningContext, IRowOrientedDataTable dataTable, INode input, DataSourceModel dataSource)
            : base(lap, learningContext, dataTable)
        {
            _Initialise(dataTable);
            _input = input;
            _inputSize = dataSource.InputSize;
            _outputSize = dataSource.OutputSize;
        }

        void _Initialise(IDataTable dataTable)
        {
            _rowDepth = new uint[dataTable.RowCount];
            Matrix<float> inputMatrix = null, outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputMatrix = (Matrix<float>)row[0];
                outputMatrix = (Matrix<float>)row[1];
                _rowDepth[i] = outputMatrix.RowCount;
            });

            _inputSize = inputMatrix.ColumnCount;
            _outputSize = outputMatrix.ColumnCount;
        }

        private SequenceToSequenceDataTableAdaptor(ILinearAlgebraProvider lap, ILearningContext learningContext, IRowOrientedDataTable dataTable, INode input, uint inputSize, uint outputSize)
            : base(lap, learningContext, dataTable)
        {
            _Initialise(dataTable);
            _input = input;
            _inputSize = inputSize;
            _outputSize = outputSize;
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
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => (uint)d.Item2).ToArray())
                .ToArray()
            ;
        }

        (IFloatMatrix, object[][]) _Encode(IExecutionContext executionContext, uint[] rows)
        {
            var data = _GetRows(rows).ToArray();

            // create the input batch
            var inputData = data.Select(r => ((Matrix<float>) r[0], (Matrix<float>) null)).ToArray();
            var encoderInput = _GetSequentialMiniBatch(rows, inputData);

            // execute the encoder
            IMiniBatchSequence sequence;
            IFloatMatrix encoderOutput = null;
            while ((sequence = encoderInput.GetNextSequence()) != null) {
                using var context = _Process(executionContext, sequence);
                if (sequence.Type == MiniBatchSequenceType.SequenceEnd)
                    encoderOutput = context.Data.GetMatrix();
            }
            return (encoderOutput, data);
        }

        public override IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var (encoderOutput, data) = _Encode(executionContext, rows);

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
            if(context.IsTraining && batch.Type == MiniBatchSequenceType.SequenceStart) {
                context.LearningContext.DeferBackpropagation(null, signal => {
                    _learningContext.BackpropagateThroughTime(signal);
                });
            }
        }
    }
}
