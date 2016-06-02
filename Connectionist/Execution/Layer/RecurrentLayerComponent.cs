using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Execution.Layer
{
    public class RecurrentLayerComponent : IDisposable
    {
        readonly List<StandardFeedForward> _part = new List<StandardFeedForward>();
        readonly IActivationFunction _activation;

        public RecurrentLayerComponent(StandardFeedForward weight, StandardFeedForward memory, IActivationFunction activation)
        {
            _activation = activation;
            _part.Add(weight);
            _part.Add(memory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var part in _part)
                    part.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IMatrix Activate(IEnumerable<IMatrix> input)
        {
            var part = input.Zip(_part, (m, w) => w.Execute(m)).ToList();
            for (var i = 1; i < part.Count; i++) {
                part[0].AddInPlace(part[1]);
                part[1].Dispose();
            }
            using (var combined = part[0]) {
                return _activation.Calculate(combined);
            }
        }
    }
}
