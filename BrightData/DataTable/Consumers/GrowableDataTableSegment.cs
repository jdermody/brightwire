using System.Collections.Generic;
using BrightData.Buffer.Hybrid;

namespace BrightData.DataTable.Consumers
{
    /// <summary>
    /// A typed data table segment that can grow in size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GrowableDataTableSegment<T> : HybridBufferSegment<T>, IConsumeColumnData<T>, IDataTableSegment<T>
        where T: notnull
    {
        public GrowableDataTableSegment(
            BrightDataContext context, 
            IColumnInfo column,
            IProvideTempStreams tempStream
        ) : base(column.ColumnType, column.MetaData, (IHybridBuffer<T>)column.ColumnType.GetHybridBufferWithMetaData(column.MetaData, context, tempStream))
        {
            Context = context;
            ColumnIndex = column.Index;
            Types = new[] { SingleType };
        }

        public IBrightDataContext Context { get; }
        public BrightDataType[] Types { get; }
        public IEnumerable<object?> Data => Enumerate();
        public uint ColumnIndex { get; }
        public BrightDataType ColumnType => SingleType;
    }
}
