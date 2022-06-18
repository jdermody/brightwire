using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Operations
{
    internal class AnalyseColumnOperation<T> : OperationBase<MetaData> where T : notnull
    {
        readonly MetaData         _metaData;
        readonly IEnumerator<T>   _enumerator;
        readonly IDataAnalyser<T> _analyser;

        public AnalyseColumnOperation(uint rowCount, MetaData metaData, IEnumerator<T> enumerator, IDataAnalyser<T> analyser) : base(rowCount, null)
        {
            _metaData = metaData;
            _enumerator = enumerator;
            _analyser = analyser;
        }

        protected override void NextStep(uint index)
        {
            if(_enumerator.MoveNext())
                _analyser.Add(_enumerator.Current);
        }

        protected override MetaData GetResult(bool wasCancelled)
        {
            _analyser.WriteTo(_metaData);
            return _metaData;
        }
    }
}
