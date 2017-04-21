using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    abstract class WireBase : IWire
    {
        protected readonly int _inputSize, _outputSize;
        protected readonly int _channel;
        protected IWire _destination;

        public event Action<WireSignalEventArgs> OnInput;
        public event Action<WireSignalEventArgs> OnOutput;

        public WireBase(int inputSize, int outputSize, int channel, IWire destination)
        {
            _destination = destination;
            _inputSize = inputSize;
            _outputSize = outputSize;
            _channel = channel;
        }

        public void SetDestination(IWire wire) => _destination = wire;
        public IWire Destination => _destination;
        public int Channel => _channel;
        public int InputSize => _inputSize;
        public int OutputSize => _outputSize;

        public abstract void Send(IMatrix signal, int channel, IBatchContext context);

        protected (IMatrix Signal, int Channel) _OnInput(IMatrix signal, int channel, IBatchContext context)
        {
            var args = new WireSignalEventArgs(signal, channel, context);
            OnInput?.Invoke(args);
            return (args.Signal, args.Channel);
        }

        protected (IMatrix Signal, int Channel) _OnOutput(IMatrix signal, int channel, IBatchContext context)
        {
            var args = new WireSignalEventArgs(signal, channel, context);
            OnOutput?.Invoke(args);
            return (args.Signal, args.Channel);
        }

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
