using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Input
{
    abstract class AdaptiveDataTableAdaptorBase : DataTableAdaptorBase
    {
        protected FlowThrough _input;
        protected readonly IExecutionContext _executionContext;
        protected readonly ILearningContext _learningContext;

        public AdaptiveDataTableAdaptorBase(ILearningContext learningContext, IDataTable dataTable, IExecutionContext executionContext)
            : base(learningContext.LinearAlgebraProvider, dataTable)
        {
            Debug.Assert(learningContext.DeferUpdates);

            _learningContext = learningContext;
            _executionContext = executionContext;
            _input = new FlowThrough();
        }

        override public INode AdaptiveInput => _input;

        protected IContext _Process(IGraphData data)
        {
            var context = new TrainingEngineContext(_executionContext, data, _learningContext, _input);
            _input.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            return context;
        }

        protected IContext _Process(IMiniBatchSequence sequence)
        {
            var context = new TrainingEngineContext(_executionContext, sequence, _learningContext, _input);
            _input.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            return context;
        }
    }
}
