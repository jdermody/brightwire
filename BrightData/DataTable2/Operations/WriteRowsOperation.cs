using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Operations
{
    internal class WriteRowsOperation : OperationBase<bool>
    {
        readonly BrightDataContext                           _context;
        readonly IEnumerator<(uint RowIndex, object[] Data)> _enumerator;
        readonly HashSet<uint>?                              _rowIndices;
        readonly Predicate<object[]>?                        _predicate;
        readonly IHybridBuffer[]                             _buffers;
        readonly IProvideTempStreams                         _tempStreams;
        readonly Stream                                      _stream;
        readonly MetaData                                    _tableMetaData;

        public WriteRowsOperation(
            BrightDataContext context,
            IEnumerator<(uint RowIndex, object[] Data)> data,
            HashSet<uint>? rowIndices,
            Predicate<object[]>? predicate,
            IHybridBuffer[] buffers,
            uint rowCount,
            IProvideTempStreams tempStreams,
            Stream stream,
            MetaData tableMetaData
        ) : base(rowCount, null)
        {
            _context       = context;
            _enumerator    = data;
            _rowIndices    = rowIndices;
            _predicate     = predicate;
            _buffers       = buffers;
            _tempStreams   = tempStreams;
            _stream        = stream;
            _tableMetaData = tableMetaData;
        }

        public override void Dispose()
        {
            _enumerator.Dispose();
            _tempStreams.Dispose();
        }

        protected override void NextStep(uint index)
        {
            if (_rowIndices is not null && !_rowIndices.Contains(index))
                return;

            // read a row into the buffer
            _enumerator.MoveNext();
            var (_, row) = _enumerator.Current;

            if (_predicate?.Invoke(row) == false)
                return;

            // write the row
            for (var i = 0; i < row.Length; i++)
                _buffers[i].AddObject(row[i]);
        }

        protected override bool GetResult(bool wasCancelled)
        {
            var writer = new BrightDataTableWriter(_context, _tempStreams, _stream);
            writer.Write(_tableMetaData, _buffers.Cast<ISingleTypeTableSegment>().ToArray());
            return !wasCancelled;
        }
    }
}
