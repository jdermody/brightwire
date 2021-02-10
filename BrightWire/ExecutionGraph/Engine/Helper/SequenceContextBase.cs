using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    abstract class SequenceContextBase
    {
        public void StoreExecutionResult()
        {
            
        }

        protected abstract IGraphSequenceContext Context { get; }

        public ExecutionResult Result 
        {
            get
            {
                var context = Context;
                var output = context.Output;
                var matrixOutput = output.Any()
                    ? output.Select(o => o.GetMatrix().Data)
                    : new[] {context.Data.GetMatrix().Data};

                return new ExecutionResult(context.BatchSequence, matrixOutput.SelectMany(m => m.Rows).ToArray());
            }
        }
    }
}
