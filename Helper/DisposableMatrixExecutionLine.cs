using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper
{
    public class DisposableMatrixExecutionLine : IDisposableMatrixExecutionLine
    {
        readonly List<IMatrix> _garbage = new List<IMatrix>();
        IMatrix _curr;

        public DisposableMatrixExecutionLine(IMatrix matrix = null)
        {
            if (matrix != null)
                Assign(matrix);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var item in _garbage)
                    item.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IMatrix Current { get { return _curr; } }

        public void Assign(IMatrix matrix)
        {
            _garbage.Add(_curr = matrix);
        }

        public void Replace(IMatrix matrix)
        {
            _curr = matrix;
        }

        public IMatrix Pop()
        {
            var ret = _garbage[_garbage.Count - 1];
            _garbage.RemoveAt(_garbage.Count - 1);
            return ret;
        }
    }
}
