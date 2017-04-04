using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    public class ToComponent : IWire
    {
        readonly IComponent _component;

        public ToComponent(IComponent component)
        {
            _component = component;
        }

        public IMatrix Send(IMatrix signal, IWireContext context)
        {
            return _component.Execute(signal, context);
        }
    }
}
