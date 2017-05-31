using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System.Diagnostics;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Base class for data table adaptors that adapt their input based on a preliminary execution graph
    /// </summary>
    abstract class AdaptiveDataTableAdaptorBase : RowBasedDataTableAdaptorBase, IAdaptiveDataSource
    {
        protected INode _input;
        protected readonly ILearningContext _learningContext;

        public AdaptiveDataTableAdaptorBase(ILinearAlgebraProvider lap, ILearningContext learningContext, IDataTable dataTable)
            : base(lap, dataTable)
        {
            Debug.Assert(learningContext == null || learningContext.DeferUpdates);

            _learningContext = learningContext;
            _input = new FlowThrough();
        }

        public INode AdaptiveInput => _input;

        public Models.DataSourceModel GetModel(string name = null)
        {
            return new Models.DataSourceModel {
                Name = name,
                InputSize = InputSize,
                OutputSize = OutputSize,
                Graph = _input.GetGraph()
            };
        }

        protected IContext _Process(IExecutionContext executionContext, IGraphData data)
        {
            var context = new TrainingEngineContext(executionContext, data, _learningContext);
            _input.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            return context;
        }

        protected IContext _ConcurentProcess(IExecutionContext executionContext, IGraphData data)
        {
            var learningContext = new LearningContext(_lap, _learningContext.LearningRate, _learningContext.BatchSize, false, true);
            var context = new TrainingEngineContext(executionContext, data, learningContext);
            _input.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            return context;
        }

        protected IContext _Process(IExecutionContext executionContext, IMiniBatchSequence sequence)
        {
            var context = new TrainingEngineContext(executionContext, sequence, _learningContext);
            _input.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            return context;
        }
    }
}
