using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    public class LayerToWire : IWire
    {
        readonly ILayer _layer;
        readonly IWire _destination;

        public LayerToWire(ILayer layer, IWire destination)
        {
            _layer = layer;
            _destination = destination;
        }

        public IMatrix Send(IMatrix signal, IWireContext context)
        {
            var result = _layer.Forward(signal);
            context.AddBackpropagation(result.BackProp);
            return _destination.Send(result.Output, context);
        }
    }
}
