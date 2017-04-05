using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    public class WireToLayer : IWire
    {
        readonly ILayer _layer;
        readonly IWire _destination;

        public WireToLayer(ILayer layer, IWire destination)
        {
            _layer = layer;
            _destination = destination;
        }

        public IMatrix Send(IMatrix signal, IWireContext context)
        {
            IMatrix ret;
            if (context != null) {
                var result = _layer.Forward(signal);
                context.AddBackpropagation(result.BackProp);
                ret = result.Output;
            } else
                ret = _layer.Execute(signal);

            return _destination.Send(ret, context);
        }
    }
}
