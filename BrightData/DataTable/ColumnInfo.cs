using System.IO;
using BrightData.DataTable.Consumers;
using BrightData.Helper;

namespace BrightData.DataTable
{
    /// <summary>
    /// Data table column information
    /// </summary>
    internal class ColumnInfo : IColumnInfo
    {
        public ColumnInfo(BinaryReader reader, uint index)
        {
            ColumnType = (BrightDataType)reader.ReadSByte();
            MetaData = new MetaData(reader);
            MetaData.Set(Consts.ColumnIndex, index);
            Index = index;
        }

        public ColumnInfo(uint index, BrightDataType type, IMetaData metaData)
        {
            Index = index;
            ColumnType = type;
            MetaData = metaData;
            MetaData.Set(Consts.ColumnIndex, index);
        }

        public uint Index { get; }
        public BrightDataType ColumnType { get; }
        public IHaveDictionary? Dictionary { get; } = null;
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
            var type = typeof(ColumnAnalyser<>).MakeGenericType(ColumnType.GetDataType());
            return GenericActivator.Create<IConsumeColumnData, ICanComplete>(type, Index, ColumnType, MetaData, GetAnalyser());
        }
    }
}
