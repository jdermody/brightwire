using System;
using System.Collections.Generic;

namespace BrightData.DataTable.Meta
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="columnIndex"></param>
    /// <param name="columnType"></param>
    /// <param name="filter"></param>
    /// <param name="nonConformingRowIndices"></param>
    internal class ColumnFilter<T>(uint columnIndex, BrightDataType columnType, IDataTypeSpecification<T> filter, HashSet<uint> nonConformingRowIndices)
        : IAppendBlocks<T>
        where T : notnull
    {
        uint _index = 0;

        public uint ColumnIndex { get; } = columnIndex;
        public BrightDataType ColumnType { get; } = columnType;

        public void Append(ReadOnlySpan<T> inputBlock)
        {
            foreach (var item in inputBlock)
            {
                if (!filter.IsValid(item))
                    nonConformingRowIndices.Add(_index);
                ++_index;
            }
        }
        public Type BlockType => typeof(T);
    }
}
