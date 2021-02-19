using System.IO;
using BrightData.Helper;

namespace BrightData.Segment
{
    internal class ColumnInfo : IColumnInfo
    {
        public class Analyser<T> : IConsumeColumnData<T>, ICanComplete where T: notnull
        {
            readonly IMetaData _metaData;
            readonly IDataAnalyser<T> _analyser;

            public uint ColumnIndex { get; }
            public ColumnType ColumnType { get; }

            public Analyser(uint columnIndex, ColumnType type, IMetaData metaData, IDataAnalyser<T> analyser)
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

        public ColumnInfo(BinaryReader reader, uint index)
        {
            ColumnType = (ColumnType)reader.ReadSByte();
            MetaData = new MetaData(reader);
            MetaData.Set(Consts.Index, index);
            Index = index;
        }

        public ColumnInfo(uint index, ColumnType type, IMetaData metaData)
        {
            Index = index;
            ColumnType = type;
            MetaData = metaData;
            MetaData.Set(Consts.Index, index);
        }

        public uint Index { get; }
        public ColumnType ColumnType { get; }
        public IMetaData MetaData { get; }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((sbyte) ColumnType);
            MetaData.WriteTo(writer);
        }

        public override string ToString()
        {
            return $"[{ColumnType}]: {MetaData}";
        }

        public IDataAnalyser GetAnalyser() => ColumnType.GetColumnAnalyser(MetaData);

        public (IConsumeColumnData, ICanComplete) GetAnalysisConsumer()
        {
            var type = typeof(Analyser<>).MakeGenericType(ColumnType.GetDataType());
            return GenericActivator.Create<IConsumeColumnData, ICanComplete>(type, Index, ColumnType, MetaData, GetAnalyser());
        }
    }
}
