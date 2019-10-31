using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;

namespace BrightTable.Segments
{
    class ColumnInfo
    {
        readonly MetaData _metaData;

        public ColumnInfo(BinaryReader reader, uint index)
        {
            ColumnType = (ColumnType)reader.ReadSByte();
            _metaData = new MetaData(reader);
            Index = index;
        }

        public uint Index { get; }
        public ColumnType ColumnType { get; }
        public IMetaData MetaData => _metaData;

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((sbyte) ColumnType);
            _metaData.WriteTo(writer);
        }

        public override string ToString()
        {
            return $"{Index}) {ColumnType} - {_metaData}";
        }
    }
}
