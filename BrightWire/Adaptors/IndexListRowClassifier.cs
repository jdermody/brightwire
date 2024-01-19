using BrightData;
using BrightData.DataTable;
using BrightData.Types;

namespace BrightWire.Adaptors
{
    internal class IndexListRowClassifier(IIndexListClassifier classifier, uint columnIndex = 0, IIndexStrings? indexer = null)
        : IRowClassifier, IHaveStringIndexer
    {
        public IIndexListClassifier Classifier { get; } = classifier;
        public uint ColumnIndex { get; } = columnIndex;

        public (string Label, float Weight)[] Classify(TableRow row)
        {
            var indexList = row.Get<IndexList>(ColumnIndex);
            return Classifier.Classify(indexList);
        }

        public IIndexStrings? Indexer { get; } = indexer;
    }
}
