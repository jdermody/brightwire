using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.DataTable.Builders;
using BrightData.Helper;
using BrightData.Transformation;

namespace BrightData.DataTable
{
    /// <summary>
    /// Data table in which the columns are stored contiguously
    /// </summary>
    internal class ColumnOrientedDataTable : DataTableBase, IColumnOrientedDataTable
    {
        interface IConsumerBinding
        {
            void Copy(uint maxRows);
        }
        class ConsumerBinding<T> : IConsumerBinding where T : notnull
        {
            readonly IDataTableSegment<T> _segment;
            readonly IConsumeColumnData<T> _consumer;

            public ConsumerBinding(ISingleTypeTableSegment segment, IConsumeColumnData consumer)
            {
                _segment = (IDataTableSegment<T>)segment;
                _consumer = (IConsumeColumnData<T>)consumer;
            }

            public void Copy(uint maxRows)
            {
                uint index = 0;
                using var enumerator = _segment.EnumerateTyped().GetEnumerator();
                while (index < maxRows && enumerator.MoveNext())
                    _consumer.Add(enumerator.Current, index++);
            }
        }
        interface IAnalyserBinding
        {
            void Analyse();
        }
        class AnalyserBinding<T> : IAnalyserBinding where T : notnull
        {
            readonly IDataAnalyser<T> _analyser;
            readonly IDataTableSegment<T> _segment;

            public AnalyserBinding(ISingleTypeTableSegment segment, IDataAnalyser analyser)
            {
                _analyser = (IDataAnalyser<T>)analyser;
                _segment = (IDataTableSegment<T>)segment;
            }

            public void Analyse()
            {
                foreach (var item in _segment.EnumerateTyped())
                    _analyser.Add(item);
            }
        }

        readonly (IColumnInfo Info, ISingleTypeTableSegment Segment)[] _columns;

        public ColumnOrientedDataTable(IBrightDataContext context, Stream stream, bool readHeader, ICloneStreams streamCloner) : base(context)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            if (readHeader)
                ReadHeader(reader, DataTableOrientation.ColumnOriented);
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            MetaData.ReadFrom(reader);

            _columns = new (IColumnInfo Info, ISingleTypeTableSegment Segment)[ColumnCount];
            ColumnTypes = new BrightDataType[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++) {
                var nextColumnPosition = reader.ReadInt64();
                _columns[i] = Load(i, reader, 32768, streamCloner);
                ColumnTypes[i] = _columns[i].Info.ColumnType;
                stream.Seek(nextColumnPosition, SeekOrigin.Begin);
            }
        }

        (IColumnInfo Info, ISingleTypeTableSegment Segment) Load(uint index, BinaryReader reader, uint inMemorySize, ICloneStreams streamCloner)
        {
            var columnType = (BrightDataType)reader.ReadByte();
            var metadata = new MetaData(reader);

            // ensure the metadata has the index and type
            metadata.Set(Consts.Index, index);
            metadata.SetType(columnType);

            // create the column
            var dataType = columnType.GetDataType();
            return GenericActivator.Create<IColumnInfo, ISingleTypeTableSegment>(typeof(Column<>).MakeGenericType(dataType),
                index,
                columnType,
                metadata,
                Context,
                streamCloner,
                inMemorySize
            );
        }

        public void Dispose()
        {
            foreach (var (_, segment) in _columns)
                segment.Dispose();
        }

        public override DataTableOrientation Orientation => DataTableOrientation.ColumnOriented;
        public BrightDataType[] ColumnTypes { get; }
        public override void ForEachRow(Action<object[]> callback, uint maxRows = uint.MaxValue)
        {
            ForEachRow((row, index) => callback(row), maxRows);
        }

        protected override IDataTable Table => this;

        public ISingleTypeTableSegment[] AllColumns()
        {
            var ret = new ISingleTypeTableSegment[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = _columns[i].Segment;
            return ret;
        }

        ISingleTypeTableSegment IDataTable.Column(uint columnIndex) => _columns[columnIndex].Segment;
        public IEnumerable<(uint ColumnIndex, IMetaData MetaData)> ColumnAnalysis(IEnumerable<uint> columnIndices) => columnIndices.Select(ci => (ci, ColumnAnalysis(ci)));

        public IMetaData ColumnAnalysis(uint columnIndex, bool force = false, uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            var segment = _columns[columnIndex].Segment;
            var ret = segment.MetaData;
            lock (_columns) {
                if (force || !ret.Get(Consts.HasBeenAnalysed, false)) {
                    var type = segment.SingleType;
                    var analyser = type.GetColumnAnalyser(segment.MetaData, writeCount, maxCount);
                    var binding = GenericActivator.Create<IAnalyserBinding>(typeof(AnalyserBinding<>).MakeGenericType(type.GetDataType()),
                        segment,
                        analyser
                    );
                    binding.Analyse();
                    analyser.WriteTo(ret);
                    ret.Set(Consts.HasBeenAnalysed, true);
                }
            }

            return ret;
        }

        public IEnumerable<(string Label, IHybridBuffer[] ColumnData)> GroupBy(params uint[] columnIndices)
        {
            var groups = new Dictionary<string, IHybridBuffer[]>();
            ForEachRow((row, rowIndex) => {
                var label = GetGroupLabel(columnIndices, row);
                if (!groups.TryGetValue(label, out var data))
                    groups.Add(label, data = _columns.Select(c => c.Info.MetaData.GetGrowableSegment(c.Info.ColumnType, Context, Context.TempStreamProvider)).ToArray());
                foreach(var (obj, buffer) in row.Zip(data))
                    buffer.Add(obj, rowIndex);
            });

            return groups.OrderBy(g => g.Key).Select(kv => (kv.Key, kv.Value));
        }

        public IMetaData ColumnMetaData(uint columnIndex) => _columns[columnIndex].Info.MetaData;
        public void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback)
        {
            var rowSet = new HashSet<uint>(rowIndices);

            lock (_columns) {
                var columns = AllColumns().Select(c => c.Enumerate().GetEnumerator()).ToArray();
                var rowCount = rowSet.Max();

                for (uint i = 0; i < rowCount + 1; i++) {
                    if (rowSet.Contains(i)) {
                        var row = new object[ColumnCount];
                        for (uint j = 0; j < ColumnCount; j++) {
                            var column = columns[j];
                            column.MoveNext();
                            row[j] = column.Current;
                        }
                        callback(row);
                    }
                }
            }
        }

        public IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
        {
            var table = new Dictionary<uint, ISingleTypeTableSegment>();
            foreach (var index in this.AllOrSelectedColumnIndices(columnIndices).OrderBy(i => i).Distinct())
                table.Add(index, _columns[index].Segment);
            return columnIndices.Select(i => table[i]);
        }

        public void ReadTyped(IEnumerable<IConsumeColumnData> consumers, uint maxRows = 4294967295U)
        {
            lock (_columns) {
                var bindings = consumers.Select(consumer => GenericActivator.Create<IConsumerBinding>(
                    typeof(ConsumerBinding<>).MakeGenericType(consumer.ColumnType.GetDataType()),
                    _columns[consumer.ColumnIndex].Segment,
                    consumer
                ));
                foreach (var binding in bindings)
                    binding?.Copy(maxRows);
            }
        }

        public void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue)
        {
            lock (_columns) {
                var columns = AllColumns().Select(c => c.Enumerate().GetEnumerator()).ToArray();
                var rowCount = Math.Min(maxRows, RowCount);

                for (uint i = 0; i < rowCount; i++) {
                    var row = new object[ColumnCount];
                    for (uint j = 0; j < ColumnCount; j++) {
                        var column = columns[j];
                        column.MoveNext();
                        row[j] = column.Current;
                    }

                    callback(row, i);
                }
            }
        }

        public IRowOrientedDataTable AsRowOriented(string? filePath = null)
        {
            using var builder = new RowOrientedTableBuilder(MetaData, RowCount, filePath);
            foreach (var (info, _) in _columns)
                builder.AddColumn(info.ColumnType, info.MetaData);

            // ReSharper disable once AccessToDisposedClosure
            ForEachRow((row, index) => builder.AddRow(row));

            return builder.Build(Context);
        }

        IColumnOrientedDataTable Transform(IEnumerable<IColumnTransformationParam> input, string? filePath)
        {
            using var tempStream = new TempStreamManager();
            var columnConversionTable = new Dictionary<uint, IColumnTransformationParam>();

            // build the map of columns to transform
            uint nextIndex = 0;
            foreach (var item in input) {
                if (item.ColumnIndex.HasValue && item.ColumnIndex.Value < ColumnCount) {
                    columnConversionTable[item.ColumnIndex.Value] = item;
                    nextIndex = item.ColumnIndex.Value + 1;
                }
                else if (nextIndex < ColumnCount)
                    columnConversionTable[nextIndex++] = item;
            }

            // create contexts for each column transformation
            var columnConversions = new Dictionary<ISingleTypeTableSegment, ITransformationContext>();
            foreach (var (info, segment) in _columns) {
                if (columnConversionTable.TryGetValue(info.Index, out var conversion)) {
                    var column = _columns[info.Index].Segment;
                    var converter = conversion.GetTransformer(info.ColumnType, column, () => ColumnAnalysis(info.Index), tempStream);
                    if (converter != null) {
                        var newColumnInfo = info.ChangeColumnType(converter.To.GetColumnType());
                        var buffer = newColumnInfo.MetaData.GetGrowableSegment(newColumnInfo.ColumnType, Context, tempStream);
                        var contextType = typeof(TransformationContext<,>).MakeGenericType(converter.From, converter.To);
                        var param = new object[] { column, converter, buffer };
                        var conversionContext = GenericActivator.Create<ITransformationContext>(contextType, param);
                        columnConversions.Add(segment, conversionContext);
                    }
                }
            }

            var convertedColumns = new List<ISingleTypeTableSegment>();
            lock (_columns) {
                for (uint i = 0; i < ColumnCount; i++) {
                    var wasConverted = false;
                    var column = _columns[i].Segment;
                    if (columnConversions.TryGetValue(column, out var converter)) {
                        var convertedCount = converter.Transform();
                        if (convertedCount == RowCount) {
                            convertedColumns.Add((ISingleTypeTableSegment)converter.Buffer);
                            wasConverted = true;
                        }
                    }
                    if (!wasConverted)
                        convertedColumns.Add(column);
                }
            }

            return convertedColumns.BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable Clone(string? filePath)
        {
            return _columns.Select(c => c.Segment).ToList().BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable Convert(string? filePath, params IColumnTransformationParam[] conversionParams)
        {
            return Transform(conversionParams, filePath);
        }

        public IColumnOrientedDataTable CopyColumns(string? filePath, params uint[] columnIndices)
        {
            return Columns(columnIndices).ToList().BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable Normalize(NormalizationType type, string? filePath = null)
        {
            if (type == NormalizationType.None)
                return this;
            var param = _columns
                .Select(c => c.Info)
                .Where(c => !c.MetaData.IsCategorical() && c.ColumnType.IsNumeric())
                .Select(c => new ColumnNormalization(c.Index, type));
            return Transform(param, filePath);
        }

        public IColumnOrientedDataTable Normalize(string? filePath, params IColumnTransformationParam[] param)
        {
            return Transform(param, filePath);
        }

        public IColumnOrientedDataTable ConcatColumns(string? filePath, params IColumnOrientedDataTable[] others)
        {
            if (others.Any(t => t.RowCount != RowCount))
                throw new ArgumentException("Row count across tables must agree");

            var columns = ColumnCount.AsRange().Select(i => _columns[i].Segment);
            foreach (var other in others)
                columns = columns.Concat(other.ColumnCount.AsRange().Select(i => other.Column(i)));

            lock (_columns) {
                return columns.ToList().BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
            }
        }

        public IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string? filePath = null)
        {
            using var tempStream = new TempStreamManager();
            var buffers = ColumnCount.AsRange()
                .Select(i => _columns[i].Info.MetaData.GetGrowableSegment(_columns[i].Info.ColumnType, Context, tempStream))
                .ToList();

            uint rowCount = 0;
            ForEachRow((row, index) => {
                if (predicate(row)) {
                    ++rowCount;
                    for (int i = 0; i < ColumnCount; i++)
                        buffers[i].Add(row[i], index);
                }
            });

            return buffers.Cast<ISingleTypeTableSegment>().ToList().BuildColumnOrientedTable(MetaData, Context, rowCount, filePath);
        }

        public IColumnOrientedDataTable ReinterpretColumns(string? filePath, params IReinterpretColumnsParam[] columns)
        {
            using var tempStream = new TempStreamManager();
            var newColumns = new List<ISingleTypeTableSegment>();
            var reinterpreted = columns.SelectMany(c => c.ColumnIndices.Select(i => (Column: c, Index: i)))
                .ToDictionary(d => d.Index, d => d.Column);

            foreach (var (info, segment) in _columns) {
                if (reinterpreted.TryGetValue(info.Index, out var rc)) {
                    if (info.Index == rc.ColumnIndices[0])
                        newColumns.AddRange(rc.GetNewColumns(Context, tempStream, (uint)newColumns.Count, rc.ColumnIndices.Select(i => _columns[i]).ToArray()));
                }
                else
                    newColumns.Add(segment);
            }

            return newColumns.BuildColumnOrientedTable(MetaData, Context, RowCount, filePath);
        }

        public override string ToString() => String.Join(", ", _columns.Select(c => c.ToString()));
    }
}
