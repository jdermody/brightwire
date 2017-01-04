using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.LinearAlgebra
{
    internal class Cpu4DTensor : I4DTensor
    {
        readonly Cpu3DTensor[] _data;
        readonly int _rows, _columns, _depth;

        public Cpu4DTensor(IReadOnlyList<I3DTensor> data)
        {
            var first = data.First();
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _columns = data.Count;
            _data = data.Cast<Cpu3DTensor>().ToArray();
        }

        void IDisposable.Dispose()
        {
            // nop
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
