using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class ColumnSplit
    {
        public IMatrix Top { get; private set; }
        public IMatrix Bottom { get; private set; }

        public ColumnSplit(IMatrix top, IMatrix bottom)
        {
            Top = top;
            Bottom = bottom;
        }
    }
}
