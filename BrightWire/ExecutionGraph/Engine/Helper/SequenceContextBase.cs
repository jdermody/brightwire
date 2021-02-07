using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class SequenceContextBase
    {
        readonly List<ExecutionResult> _results = new List<ExecutionResult>();

        public void StoreExecutionResult()
        {

        }

        public IEnumerable<ExecutionResult> Results => _results;
    }
}
