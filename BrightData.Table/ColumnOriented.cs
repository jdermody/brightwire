using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table
{
    internal partial class ColumnOriented : TableBase
    {
        readonly uint[] _columnOffset;

        public ColumnOriented(IByteReader data) : base(data, DataTableOrientation.ColumnOriented)
        {
            // determine column offsets
            _columnOffset = new uint[ColumnCount];
            _columnOffset[0] = _header.DataOffset;
            for (uint i = 1; i < _columns.Length; i++)
                _columnOffset[i] = _columnOffset[i-1] + _columns[i-1].DataTypeSize * RowCount;
        }
    }
}