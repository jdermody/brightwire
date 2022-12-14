using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData.DataTable.Operations
{
    internal class WriteRowsOperation : OperationBase<Stream?>
    {
        readonly BrightDataContext               _context;
        readonly IEnumerator<BrightDataTableRow> _enumerator;
        readonly Predicate<BrightDataTableRow>?  _predicate;
        readonly ICompositeBuffer[]                 _buffers;
        readonly IProvideTempStreams             _tempStreams;
        readonly Stream                          _stream;
        readonly MetaData                        _tableMetaData;

        public WriteRowsOperation(
            BrightDataContext context,
            IEnumerator<BrightDataTableRow> data,
            Predicate<BrightDataTableRow>? predicate,
            ICompositeBuffer[] buffers,
            uint rowCount,
            IProvideTempStreams tempStreams,
            Stream stream,
            MetaData tableMetaData
        ) : base(rowCount, null)
        {
            _context       = context;
            _enumerator    = data;
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
            // read a row into the buffer
            _enumerator.MoveNext();
            var row = _enumerator.Current;

            if (_predicate?.Invoke(row) == false)
                return;

            // write the row
            for (uint i = 0, len = row.Size; i < len; i++)
                _buffers[i].AddObject(row[i]);
        }

        protected override Stream? GetResult(bool wasCancelled)
        {
            if (wasCancelled) {
                _stream.Dispose();
                return null;
            }

            var writer = new BrightDataTableWriter(_context, _tempStreams, _stream);
            writer.Write(_tableMetaData, _buffers.Cast<ITypedSegment>().ToArray());
            return _stream;
        }
    }
}
