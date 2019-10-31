using System;
using System.Collections.Generic;
using System.Text;
using BrightData;

namespace BrightTable
{
    abstract class DataTableBase : IHaveMetaData
    {
        readonly MetaData _tableMetaData = new MetaData();
        protected const uint PREVIEW_SIZE = 10;

        public DataTableBase(IBrightDataContext context)
        {
            Context = context;
        }

        internal static string DefaultColumnName(string name, int numColumns)
        {
            return name ?? $"Column {numColumns + 1}";
        }

        public IBrightDataContext Context { get; }
        public uint RowCount { get; protected set; }
        public uint ColumnCount { get; protected set; }
        public IMetaData MetaData => _tableMetaData;
    }
}
