using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable.Operations
{
    internal class AnalyseColumnWithCastOperation<T, T2> : OperationBase<(uint, MetaData)> 
        where T : notnull 
        where T2 : class
    {
        readonly uint                       _columnIndex;
        readonly MetaData                   _metaData;
        readonly ICanEnumerateDisposable<T> _reader;
        readonly IEnumerator<T>             _enumerator;
        readonly IDataAnalyser<T2>          _analyser;

        public AnalyseColumnWithCastOperation(uint rowCount, uint columnIndex, MetaData metaData, ICanEnumerateDisposable<T> reader, IDataAnalyser<T2> analyser) : base(rowCount, null)
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
            if (_enumerator.MoveNext()) {
                var val = _enumerator.Current;
                _analyser.Add(Unsafe.As<T2>(val));
            }
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
