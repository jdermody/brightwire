using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    abstract class WireBase : IWire
    {
        protected readonly int _inputSize, _outputSize;
        protected readonly int? _inputChannel;
        protected IWire _destination;

        public WireBase(int inputSize, int outputSize, int? inputChannel, IWire destination)
        {
            _destination = destination;
            _inputSize = inputSize;
            _outputSize = outputSize;
            _inputChannel = inputChannel;
        }

        public void SetDestination(IWire wire) => _destination = wire;

        public IWire Destination => _destination;
        public int? InputChannel => _inputChannel;
        abstract public void Send(IMatrix signal, int channel, IBatchContext context);

        public int InputSize => _inputSize;
        public int OutputSize => _outputSize;

        public IWire LastWire
        {
            get
            {
                IWire p = this;
                while (p.Destination != null)
                    p = p.Destination;
                return p;
            }
        }
    }
}
