using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    public class WireToComponent : IWire
    {
        readonly IComponent _component;
        readonly IWire _destination;

        public WireToComponent(IComponent component, IWire destination)
        {
            _component = component;
            _destination = destination;
        }

        public IMatrix Send(IMatrix signal, IWireContext context)
        {
            var output = _component.Execute(signal, context);
            if (_destination != null)
                return _destination.Send(output, context);
            else
                return output;
        }
    }
}
