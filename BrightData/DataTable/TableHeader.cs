namespace BrightData.DataTable
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
