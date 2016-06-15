using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Training.Helper
{
    public class UpdateAccumulator : INeuralNetworkUpdateAccumulator
    {
        readonly ITrainingContext _context;
        readonly List<Tuple<INeuralNetworkLayerUpdater, IMatrix, IMatrix>> _update = new List<Tuple<INeuralNetworkLayerUpdater, IMatrix, IMatrix>>();
        readonly Dictionary<string, IMatrix> _data = new Dictionary<string, IMatrix>();

        public UpdateAccumulator(ITrainingContext context)
        {
            _context = context;
        }

        public IMatrix GetData(string name)
        {
            IMatrix ret;
            if (_data.TryGetValue(name, out ret))
                return ret;
            return null;
        }

        public void SetData(string name, IMatrix data)
        {
            var existing = GetData(name);
            if (existing != null)
                existing.Dispose();
            _data[name] = data;
        }

        public void Clear()
        {
            foreach (var item in _data)
                item.Value.Dispose();
            _data.Clear();
        }

        public ITrainingContext Context { get { return _context; } }

        public void Record(INeuralNetworkLayerUpdater updater, IMatrix bias, IMatrix weights)
        {
            _update.Add(Tuple.Create(updater, bias, weights));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var item in _update) {
                    item.Item1.Update(item.Item2, item.Item3, _context);
                    item.Item2.Dispose();
                    item.Item3.Dispose();
                }

                Clear();
                _update.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
