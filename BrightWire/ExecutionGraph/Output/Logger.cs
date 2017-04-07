using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Output
{
    class Logger : IComponent
    {
        readonly Action<IMatrix> _capture;

        public Logger(Action<IMatrix> capture)
        {
            _capture = capture;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            _capture(input);
            return input;
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            return Execute(input, channel, context);
        }
    }
}
