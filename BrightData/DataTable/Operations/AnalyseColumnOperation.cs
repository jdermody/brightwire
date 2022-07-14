using System.Collections.Generic;

namespace BrightData.DataTable.Operations
{
    internal class AnalyseColumnOperation<T> : OperationBase<(uint, MetaData)> where T : notnull
    {
        readonly uint                       _columnIndex;
        readonly MetaData                   _metaData;
        readonly ICanEnumerateDisposable<T> _reader;
        readonly IEnumerator<T>             _enumerator;
        readonly IDataAnalyser<T>           _analyser;

        public AnalyseColumnOperation(uint rowCount, uint columnIndex, MetaData metaData, ICanEnumerateDisposable<T> reader, IDataAnalyser<T> analyser) : base(rowCount, null)
        {
            _columnIndex = columnIndex;
            _metaData = metaData;
            _reader = reader;
            _enumerator = _reader.Values.GetEnumerator();
            _analyser = analyser;
        }

        public override void Dispose()
        {
            _reader.Dispose();
        }

        protected override void NextStep(uint index)
        {
            if(_enumerator.MoveNext())
                _analyser.Add(_enumerator.Current);
        }

        protected override (uint, MetaData) GetResult(bool wasCancelled)
        {
            if(!wasCancelled)
                _metaData.Set(Consts.HasBeenAnalysed, true);

            _analyser.WriteTo(_metaData);
            return (_columnIndex, _metaData);
        }
    }
}
