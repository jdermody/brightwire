using System;
using System.Collections.Generic;
using System.Text;

namespace BrightTable.Helper
{
    public class OneHotEncodings
    {
        public OneHotEncodings(uint columnIndex, string[] categories)
        {
            ColumnIndex = columnIndex;
            Categories = categories;
        }

        public uint ColumnIndex { get; }
        public string[] Categories { get; }
    }
}
