using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer.Recurrent
{
    internal class Bidirectional : INeuralNetworkBidirectionalLayer
    {
        readonly INeuralNetworkRecurrentLayer _forward, _backward;

        public Bidirectional(INeuralNetworkRecurrentLayer forward, INeuralNetworkRecurrentLayer backward = null)
        {
            _forward = forward;
            _backward = backward;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _forward.Dispose();
                _backward?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public INeuralNetworkRecurrentLayer Forward { get { return _forward; } }
        public INeuralNetworkRecurrentLayer Backward { get { return _backward; } }

        public BidirectionalLayer LayerInfo
        {
            get
            {
                return new BidirectionalLayer {
                    Forward = _forward?.LayerInfo,
                    Backward = _backward?.LayerInfo
                };
            }

            set
            {
                if(value.Forward != null)
                    _forward.LayerInfo = value.Forward;
                if(value.Backward != null)
                    _backward.LayerInfo = value.Backward;
            }
        }
    }
}
