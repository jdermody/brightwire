using BrightData;
using BrightData.DataTable;

namespace BrightWire.Adaptors
{
    internal class IndexListRowClassifier : IRowClassifier, IHaveStringIndexer
    {
        public IIndexListClassifier Classifier { get; }
        public uint ColumnIndex { get; }

        public IndexListRowClassifier(IIndexListClassifier classifier, uint columnIndex = 0, IIndexStrings? indexer = null)
        {
            Classifier = classifier;
            ColumnIndex = columnIndex;
            Indexer = indexer;
        }

        public (string Label, float Weight)[] Classify(TableRow row)
        {
            var indexList = row.Get<IndexList>(ColumnIndex);
            return Classifier.Classify(indexList);
        }

        public IIndexStrings? Indexer { get; }
    }
}
