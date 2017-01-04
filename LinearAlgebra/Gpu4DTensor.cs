using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.LinearAlgebra
{
    internal class Gpu4DTensor : I4DTensor
    {
        readonly Gpu3DTensor[] _data;
        readonly int _rows, _columns, _depth;

        public Gpu4DTensor(IReadOnlyList<I3DTensor> data)
        {
            var first = data.First();
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _columns = data.Count;
            _data = data.Cast<Gpu3DTensor>().ToArray();
        }

        ~Gpu4DTensor()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (var item in _data)
                item.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int ColumnCount
        {
            get
            {
                return _columns;
            }
        }

        public int Count
        {
            get
            {
                return _data.Length;
            }
        }

        public int Depth
        {
            get
            {
                return _depth;
            }
        }

        public int RowCount
        {
            get
            {
                return _rows;
            }
        }

        public I3DTensor Get(int index)
        {
            return _data[index];
        }
    }
}
