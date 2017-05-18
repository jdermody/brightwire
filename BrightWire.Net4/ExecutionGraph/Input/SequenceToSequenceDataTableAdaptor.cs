using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Input
{
    class SequenceToSequenceDataTableAdaptor : AdaptiveDataTableAdaptorBase
    {
        int[] _rowDepth;
        int _inputSize, _outputSize;
        //readonly List<IContext> _batchEncoder = new List<IContext>();

        public SequenceToSequenceDataTableAdaptor(ILearningContext learningContext, IExecutionContext executionContext, GraphFactory factory, IDataTable dataTable, Action<WireBuilder> dataConversionBuilder)
            : base(learningContext, dataTable, executionContext)
        {
            _Initialise(dataTable);

            var wireBuilder = factory.Build(_inputSize, _input);
            dataConversionBuilder(wireBuilder);

            // execute the graph to find the input size (which is the size of the adaptive graph's output)
            var output = _Encode(new[] { 0 });
            _inputSize = output.Item1.ColumnCount;
            _learningContext.Clear();
        }

        public SequenceToSequenceDataTableAdaptor(ILearningContext learningContext, IExecutionContext executionContext, IDataTable dataTable, INode input, DataSourceModel dataSource)
            : base(learningContext, dataTable, executionContext)
        {
            _Initialise(dataTable);
            _input = input;
            _inputSize = dataSource.InputSize;
            _outputSize = dataSource.OutputSize;
        }

        void _Initialise(IDataTable dataTable)
        {
            _rowDepth = new int[dataTable.RowCount];
            FloatMatrix inputMatrix = null, outputMatrix = null;
            dataTable.ForEach((row, i) => {
                inputMatrix = row.GetField<FloatMatrix>(0);
                outputMatrix = row.GetField<FloatMatrix>(1);
                _rowDepth[i] = outputMatrix.RowCount;
            });

            _inputSize = inputMatrix.ColumnCount;
            _outputSize = outputMatrix.ColumnCount;
        }

        private SequenceToSequenceDataTableAdaptor(ILearningContext learningContext, IExecutionContext executionContext, IDataTable dataTable, INode input, int inputSize, int outputSize)
            : base(learningContext, dataTable, executionContext)
        {
            _Initialise(dataTable);
            _input = input;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        public override IDataSource GetFor(IDataTable dataTable)
        {
            return new SequenceToSequenceDataTableAdaptor(_learningContext, _executionContext, dataTable, _input, _inputSize, _outputSize);
        }

        public override bool IsSequential => true;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;

        public override IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => d.Item2).ToList())
                .ToList()
            ;
        }

        (IMatrix, IReadOnlyList<IRow>) _Encode(IReadOnlyList<int> rows)
        {
            var data = _dataTable.GetRows(rows);

            // create the input batch
            var inputData = new List<(FloatMatrix Input, FloatMatrix Output)>();
            foreach (var row in data)
                inputData.Add((row.GetField<FloatMatrix>(0), null));
            var encoderInput = _GetSequentialMiniBatch(rows, inputData);

            // execute the encoder
            IMiniBatchSequence sequence;
            IMatrix encoderOutput = null;
            while ((sequence = encoderInput.GetNextSequence()) != null) {
                var context = _Process(sequence);
                if (sequence.Type == MiniBatchType.SequenceEnd)
                    encoderOutput = context.Data.GetMatrix();
            }
            return (encoderOutput, data);
        }

        public override IMiniBatch Get(IReadOnlyList<int> rows)
        {
            (var encoderOutput, var data) = _Encode(rows);

            // create the decoder input
            List<FloatVector> temp;
            var outputData = new Dictionary<int, List<FloatVector>>();
            foreach (var item in data) {
                var output = item.GetField<FloatMatrix>(1);
                for (int i = 0, len = output.RowCount; i < len; i++) {
                    if (!outputData.TryGetValue(i, out temp))
                        outputData.Add(i, temp = new List<FloatVector>());
                    temp.Add(output.Row[i]);
                }
            }
            var miniBatch = new MiniBatch(rows, this);
            var curr = encoderOutput;
            foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                var output = _lap.CreateMatrix(item.Value);
                var type = (item.Key == 0)
                    ? MiniBatchType.SequenceStart
                    : item.Key == (outputData.Count - 1)
                        ? MiniBatchType.SequenceEnd
                        : MiniBatchType.Standard;
                miniBatch.Add(type, curr, output);
                curr = output;
            }
            return miniBatch;
        }

        public override void OnBatchProcessed(IContext context)
        {
            var batch = context.BatchSequence;
            if(context.IsTraining && batch.Type == MiniBatchType.SequenceStart) {
                //_learningContext.BackpropagateThroughTime(context.ErrorSignal);
                context.LearningContext.DeferBackpropagation(null, signal => {
                    _learningContext.BackpropagateThroughTime(signal);
                });
            }
        }
    }
}
