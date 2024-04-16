using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Helper
{
    internal struct TableHeader
    {
        public byte Version;
        public DataTableOrientation Orientation;
        public uint ColumnCount;
        public uint RowCount;

        public uint InfoOffset;
        public uint InfoSizeBytes;

        public uint DataOffset;
        public uint DataSizeBytes;

        public uint StringOffset;
        public uint StringSizeBytes;

        public uint TensorOffset;
        public uint TensorSizeBytes;

        public uint BinaryDataOffset;
        public uint BinaryDataSizeBytes;

        public uint IndexOffset;
        public uint IndexSizeBytes;

        public uint WeightedIndexOffset;
        public uint WeightedIndexSizeBytes;

        public uint MetaDataOffset;
        public uint MetaDataSizeBytes;
    }
}
