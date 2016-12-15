using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.LinearAlgebra
{
    internal class Gpu4DTensor : I4DTensor
    {
        readonly Gpu3DTensor[] _data;
        readonly int _rows, _columns, _depth, _count;

        public Gpu4DTensor(IReadOnlyList<I3DTensor> data)
        {
            var first = data.First();
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _columns = data.Count;
            _data = data.Cast<Gpu3DTensor>().ToArray();
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
                return _count;
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
