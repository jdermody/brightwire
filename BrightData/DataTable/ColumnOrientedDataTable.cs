using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BrightData.DataTable.Builders;
using BrightData.Helper;
using BrightData.Transformation;

namespace BrightData.DataTable
{
    /// <summary>
    /// Data table in which the columns are stored contiguously
    /// </summary>
    //internal class ColumnOrientedDataTable : DataTableBase, IColumnOrientedDataTable
    //{
    //    interface IConsumerBinding
    //    {
    //        void Copy(uint maxRows, CancellationToken ct = default);
    //    }
    //    class ConsumerBinding<T> : IConsumerBinding where T : notnull
    //    {
    //        readonly IDataTableSegment<T> _segment;
    //        readonly IConsumeColumnData<T> _consumer;

    //        public ConsumerBinding(ISingleTypeTableSegment segment, IConsumeColumnData consumer)
    //        {
    //            _segment = (IDataTableSegment<T>)segment;
    //            _consumer = (IConsumeColumnData<T>)consumer;
    //        }

    //        public void Copy(uint maxRows, CancellationToken ct)
    //        {
    //            uint index = 0;
    //            using var enumerator = _segment.EnumerateTyped().GetEnumerator();
    //            while (index < maxRows && enumerator.MoveNext() && !ct.IsCancellationRequested) {
    //                _consumer.Add(enumerator.Current);
    //                index++;
    //            }
    //        }
    //    }
    //    interface IAnalyserBinding
    //    {
    //        void Analyse(CancellationToken ct = default);
    //    }
    //    class AnalyserBinding<T> : IAnalyserBinding where T : notnull
    //    {
    //        readonly IDataAnalyser<T> _analyser;
    //        readonly IDataTableSegment<T> _segment;

    //        public AnalyserBinding(ISingleTypeTableSegment segment, IDataAnalyser analyser)
    //        {
    //            _analyser = (IDataAnalyser<T>)analyser;
    //            _segment = (IDataTableSegment<T>)segment;
    //        }

    //        public void Analyse(CancellationToken ct)
    //        {
    //            foreach (var item in _segment.EnumerateTyped()) {
    //                if (ct.IsCancellationRequested)
    //                    break;
    //                _analyser.Add(item);
    //            }
    //        }
    //    }

    //    readonly (IColumnInfo Info, ISingleTypeTableSegment Segment)[] _columns;

    //    public ColumnOrientedDataTable(IBrightDataContext context, Stream stream, bool readHeader, ICloneStreams streamCloner) : base(context)
    //    {
    //        using var reader = new BinaryReader(stream, Encoding.UTF8);
    //        if (readHeader)
    //            ReadHeader(reader, DataTableOrientation.ColumnOriented);
    //        ColumnCount = reader.ReadUInt32();
    //        RowCount = reader.ReadUInt32();
    //        MetaData.ReadFrom(reader);

    //        _columns = new (IColumnInfo Info, ISingleTypeTableSegment Segment)[ColumnCount];
    //        ColumnTypes = new BrightDataType[ColumnCount];
    //        for (uint i = 0; i < ColumnCount; i++) {
    //            var nextColumnPosition = reader.ReadInt64();
    //            _columns[i] = Load(i, reader, 32768, streamCloner);
    //            ColumnTypes[i] = _columns[i].Info.ColumnType;
    //            stream.Seek(nextColumnPosition, SeekOrigin.Begin);
    //        }
    //    }

    //    (IColumnInfo Info, ISingleTypeTableSegment Segment) Load(uint index, BinaryReader reader, uint inMemorySize, ICloneStreams streamCloner)
    //    {
    //        var columnType = (BrightDataType)reader.ReadByte();
    //        var metadata = new MetaData(reader);

    //        // ensure the metadata has the index and type
    //        metadata.Set(Consts.Index, index);
    //        metadata.SetType(columnType);

    //        // create the column
    //        var dataType = columnType.GetDataType();
    //        return GenericActivator.Create<IColumnInfo, ISingleTypeTableSegment>(typeof(Column<>).MakeGenericType(dataType),
    //            index,
    //            columnType,
    //            metadata,
    //            Context,
    //            streamCloner,
    //            inMemorySize
    //        );
    //    }

    //    public void Dispose()
    //    {
    //        foreach (var (_, segment) in _columns)
    //            segment.Dispose();
    //    }

    //    public override DataTableOrientation Orientation => DataTableOrientation.ColumnOriented;
    //    public BrightDataType[] ColumnTypes { get; }
    //    public override void ForEachRow(Action<object[]> callback, uint maxRows = uint.MaxValue)
    //    {
    //        ForEachRow((row, _) => callback(row), maxRows);
    //    }

    //    protected override IDataTable Table => this;

    //    public ISingleTypeTableSegment[] AllColumns()
    //    {
    //        var ret = new ISingleTypeTableSegment[ColumnCount];
    //        for (uint i = 0; i < ColumnCount; i++)
    //            ret[i] = _columns[i].Segment;
    //        return ret;
    //    }

    //    ISingleTypeTableSegment IDataTable.Column(uint columnIndex) => _columns[columnIndex].Segment;
    //    public IEnumerable<(uint ColumnIndex, IMetaData MetaData)> ColumnAnalysis(IEnumerable<uint> columnIndices) => columnIndices.Select(ci => (ci, ColumnAnalysis(ci)));

    //    public IMetaData ColumnAnalysis(uint columnIndex, bool force = false, uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
    //    {
    //        var segment = _columns[columnIndex].Segment;
    //        var ret = segment.MetaData;
    //        lock (_columns) {
    //            if (force || !ret.Get(Consts.HasBeenAnalysed, false)) {
    //                var type = segment.SingleType;
    //                var analyser = type.GetColumnAnalyser(segment.MetaData, writeCount, maxCount);
    //                var binding = GenericActivator.Create<IAnalyserBinding>(typeof(AnalyserBinding<>).MakeGenericType(type.GetDataType()),
    //                    segment,
    //                    analyser
    //                );
    //                binding.Analyse(Context.CancellationToken);
    //                analyser.WriteTo(ret);
    //                ret.Set(Consts.HasBeenAnalysed, true);
    //            }
    //        }

    //        return ret;
    //    }

    //    public IEnumerable<(string Label, IHybridBuffer[] ColumnData)> GroupBy(params uint[] columnIndices)
    //    {
    //        var groups = new Dictionary<string, IHybridBuffer[]>();
    //        using var tempStreams = Context.CreateTempStreamProvider();
    //        ForEachRow(row => {
    //            var label = GetGroupLabel(columnIndices, row);
    //            if (!groups.TryGetValue(label, out var data))
    //                groups.Add(label, data = _columns.Select(c => c.Info.MetaData.GetGrowableSegment(c.Info.ColumnType, Context, tempStreams)).ToArray());
    //            foreach(var (obj, buffer) in row.Zip(data))
    //                buffer.AddObject(obj);
    //        });

    //        return groups.OrderBy(g => g.Key).Select(kv => (kv.Key, kv.Value));
    //    }

    //    public IMetaData ColumnMetaData(uint columnIndex) => _columns[columnIndex].Info.MetaData;
    //    public void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback)
    //    {
    //        var rowSet = new HashSet<uint>(rowIndices);

    //        lock (_columns) {
    //            var columns = AllColumns().Select(c => c.Enumerate().GetEnumerator()).ToArray();
    //            var rowCount = rowSet.Max();
    //            var ct = Context.CancellationToken;

    //            for (uint i = 0; i < rowCount + 1 && !ct.IsCancellationRequested; i++) {
    //                if (!rowSet.Contains(i)) 
    //                    continue;

    //                var row = new object[ColumnCount];
    //                for (uint j = 0; j < ColumnCount; j++) {
    //                    var column = columns[j];
    //                    column.MoveNext();
    //                    row[j] = column.Current;
    //                }
    //                callback(row);
    //            }
    //        }
    //    }

    //    public IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
    //    {
    //        var columnIndexList = this.AllOrSelectedColumnIndices(columnIndices).ToList();
    //        var table = columnIndexList.OrderBy(i => i).Distinct().ToDictionary(index => index, index => _columns[index].Segment);
    //        return columnIndexList.Select(i => table[i]);
    //    }

    //    public void ReadTyped(IEnumerable<IConsumeColumnData> consumers, uint maxRows = 4294967295U)
    //    {
    //        lock (_columns) {
    //            var bindings = consumers.Select(consumer => GenericActivator.Create<IConsumerBinding>(
    //                typeof(ConsumerBinding<>).MakeGenericType(consumer.ColumnType.GetDataType()),
    //                _columns[consumer.ColumnIndex].Segment,
    //                consumer
    //            ));
    //            foreach (var binding in bindings)
    //                binding.Copy(maxRows, Context.CancellationToken);
    //        }
    //    }

    //    public void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue)
    //    {
    //        lock (_columns) {
    //            var columns = AllColumns().Select(c => c.Enumerate().GetEnumerator()).ToArray();
    //            var rowCount = Math.Min(maxRows, RowCount);
    //            var ct = Context.CancellationToken;

    //            for (uint i = 0; i < rowCount && !ct.IsCancellationRequested; i++) {
    //                var row = new object[ColumnCount];
    //                for (uint j = 0; j < ColumnCount; j++) {
    //                    var column = columns[j];
    //                    column.MoveNext();
    //                    row[j] = column.Current;
    //                }

    //                callback(row, i);
    //            }
    //        }
    //    }

    //    public IRowOrientedDataTable AsRowOriented(string? filePath = null)
    //    {
    //        using var builder = new RowOrientedTableBuilder(MetaData, RowCount, filePath);
    //        foreach (var (info, _) in _columns)
    //            builder.AddColumn(info.ColumnType, info.MetaData);

    //        // ReSharper disable once AccessToDisposedClosure
    //        ForEachRow((row, _) => builder.AddRow(row));

    //        return builder.Build(Context);
    //    }

    //    public IColumnOrientedDataTable Transform(IEnumerable<(uint ColumnIndex, IConvertColumn Transformer)> transformations, string? filePath)
    //    {
    //        using var tempStreams = Context.CreateTempStreamProvider();
    //        var transformationTable = transformations.ToDictionary(d => d.ColumnIndex, d => d.Transformer);

    //        // create contexts for each column transformation
    //        var columnConversions = new Dictionary<ISingleTypeTableSegment, ITransformationContext>();
    //        foreach (var (info, segment) in _columns) {
    //            if (transformationTable.TryGetValue(info.Index, out var transformer)) {
    //                var newColumnInfo = info.ChangeColumnType(transformer.To.GetBrightDataType());
    //                var buffer = newColumnInfo.MetaData.GetGrowableSegment(newColumnInfo.ColumnType, Context, tempStreams);
    //                var contextType = typeof(TransformationContext<,>).MakeGenericType(transformer.From, transformer.To);
    //                var param = new object[] { segment, transformer, buffer };
    //                var conversionContext = GenericActivator.Create<ITransformationContext>(contextType, param);
    //                columnConversions.Add(segment, conversionContext);
    //            }
    //        }

    //        var operationId = Guid.NewGuid().ToString("n");
    //        Context.UserNotifications?.OnStartOperation(operationId);
    //        var convertedColumns = new List<ISingleTypeTableSegment>();
    //        lock (_columns) {
    //            for (uint i = 0; i < ColumnCount; i++) {
    //                var wasConverted = false;
    //                var column = _columns[i].Segment;
    //                if (columnConversions.TryGetValue(column, out var converter)) {
    //                    var i1 = i;
    //                    var convertedCount = converter.Transform(progress => Context.UserNotifications.NotifyProgress(operationId, i1, ColumnCount, progress), Context.CancellationToken);
    //                    if (convertedCount == RowCount) {
    //                        convertedColumns.Add((ISingleTypeTableSegment)converter.Buffer);
    //                        wasConverted = true;
    //                    }
    //                }
    //                if (!wasConverted)
    //                    convertedColumns.Add(column);
    //            }
    //        }
    //        Context.UserNotifications?.OnCompleteOperation(operationId, Context.CancellationToken.IsCancellationRequested);

    //        return convertedColumns.BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
    //    }

    //    public IColumnOrientedDataTable Clone(string? filePath)
    //    {
    //        return _columns.Select(c => c.Segment).ToList().BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
    //    }

    //    public IColumnOrientedDataTable CopyColumns(string? filePath, params uint[] columnIndices)
    //    {
    //        return Columns(columnIndices).ToList().BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
    //    }

    //    public IColumnOrientedDataTable ConcatColumns(string? filePath, params IColumnOrientedDataTable[] others)
    //    {
    //        if (others.Any(t => t.RowCount != RowCount))
    //            throw new ArgumentException("Row count across tables must agree");

    //        var columns = ColumnCount.AsRange().Select(i => _columns[i].Segment);
    //        foreach (var other in others)
    //            columns = columns.Concat(other.ColumnCount.AsRange().Select(i => other.Column(i)));

    //        lock (_columns) {
    //            return columns.ToList().BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
    //        }
    //    }

    //    public IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string? filePath = null)
    //    {
    //        using var tempStream = Context.CreateTempStreamProvider();
    //        var buffers = ColumnCount.AsRange()
    //            .Select(i => _columns[i].Info.MetaData.GetGrowableSegment(_columns[i].Info.ColumnType, Context, tempStream))
    //            .ToList();

    //        uint rowCount = 0;
    //        ForEachRow((row, index) => {
    //            if (predicate(row)) {
    //                ++rowCount;
    //                for (int i = 0; i < ColumnCount; i++)
    //                    buffers[i].AddObject(row[i]);
    //            }
    //        });

    //        return buffers.Cast<ISingleTypeTableSegment>().ToList().BuildColumnOrientedTable(MetaData, Context, rowCount, filePath);
    //    }

    //    public IColumnOrientedDataTable ReinterpretColumns(string? filePath, params IReinterpretColumns[] columns)
    //    {
    //        using var tempStream = Context.CreateTempStreamProvider();
    //        var newColumns = new List<ISingleTypeTableSegment>();
    //        var reinterpreted = columns.SelectMany(c => c.SourceColumnIndices.Select(i => (Column: c, Index: i)))
    //            .ToDictionary(d => d.Index, d => d.Column);

    //        foreach (var (info, segment) in _columns) {
    //            if (reinterpreted.TryGetValue(info.Index, out var rc)) {
    //                if (info.Index == rc.SourceColumnIndices[0])
    //                    newColumns.AddRange(rc.GetNewColumns(Context, tempStream, (uint)newColumns.Count, rc.SourceColumnIndices.Select(i => _columns[i]).ToArray()));
    //            }
    //            else
    //                newColumns.Add(segment);
    //        }

    //        return newColumns.BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
    //    }

    //    public override string ToString() => String.Join(", ", _columns.Select(c => c.ToString()));
    //}
}
