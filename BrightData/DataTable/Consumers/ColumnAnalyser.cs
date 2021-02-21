namespace BrightData.DataTable.Consumers
{
    internal class ColumnAnalyser<T> : IConsumeColumnData<T>, ICanComplete 
        where T: notnull
    {
        readonly IMetaData _metaData;
        readonly IDataAnalyser<T> _analyser;

        public uint ColumnIndex { get; }
        public ColumnType ColumnType { get; }

        public ColumnAnalyser(uint columnIndex, ColumnType type, IMetaData metaData, IDataAnalyser<T> analyser)
        {
            _metaData = metaData;
            _analyser = analyser;
            ColumnIndex = columnIndex;
            ColumnType = type;
        }

        public void Add(T value)
        {
            _analyser.Add(value);
        }

        public void Complete()
        {
            _analyser.WriteTo(_metaData);
        }
    }
}
