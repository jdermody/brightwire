using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class RowSplit
    {
        public IMatrix Left { get; private set; }
        public IMatrix Right { get; private set; }

        public RowSplit(IMatrix left, IMatrix right)
        {
            Left = left;
            Right = right;
        }
    }
}
