using BrightWire.Models;
using BrightWire.Models.DataTable;
using BrightWire.Source.Models.DataTable;
using BrightWire.TabularData.Analysis;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightWire.Source.TabularData.Manipulation;

namespace BrightWire.TabularData
{
	/// <summary>
	/// Data table
	/// </summary>
	class DataTable : IDataTable
	{
		class Column : IColumn
		{
			bool? _isContinuous;

			public Column(int index, string name, ColumnType type, int numDistinct, bool? isContinuous, bool isTarget)
			{
				Index = index;
				Name = name;
				Type = type;
				NumDistinct = numDistinct;
				_isContinuous = isContinuous;
				IsTarget = isTarget;
			}
			public int Index { get; }
			public ColumnType Type { get; }
			public string Name { get; }
			public bool IsTarget { get; set; }
			public int? DimensionX { get; set; }
			public int? DimensionY { get; set; }
			public int? DimensionZ { get; set; }
			public int NumDistinct { get; }

			public bool IsContinuous
			{
				get => _isContinuous ?? ColumnTypeClassifier.IsContinuous(this);
				set => _isContinuous = value;
			}
		}

		readonly List<Column> _column = new List<Column>();
		readonly long _dataOffset;
		protected readonly Stream _stream;
		readonly object _mutex = new object();
		readonly IReadOnlyList<long> _index;
		readonly RowConverter _rowConverter = new RowConverter();
		readonly int _rowCount, _blockSize;

		IDataTableAnalysis _analysis = null;

		public DataTable(Stream stream, IReadOnlyList<long> dataIndex, int rowCount)
		{
			_index = dataIndex;
			_rowCount = rowCount;
			var reader = new BinaryReader(stream, Encoding.UTF8, true);
			reader.ReadInt32(); // read the format version
			_blockSize = reader.ReadInt32(); // read the block size
			var columnCount = reader.ReadInt32(); // read the number of columns
			for (var i = 0; i < columnCount; i++) {
				var name = reader.ReadString();
				var type = (ColumnType)reader.ReadByte();
				var isTarget = reader.ReadBoolean();
				var numDistinct = reader.ReadInt32();
				var continuousSpecified = reader.ReadBoolean();
				bool? isContinuous = null;
				if (continuousSpecified)
					isContinuous = reader.ReadBoolean();
				var column = new Column(_column.Count, name, type, numDistinct, isContinuous, isTarget);
				if (type == ColumnType.Vector || type == ColumnType.Matrix || type == ColumnType.Tensor)
					column.DimensionX = _ReadPositiveNullableInt(reader);
				if (type == ColumnType.Matrix || type == ColumnType.Tensor)
					column.DimensionY = _ReadPositiveNullableInt(reader);
				if (type == ColumnType.Tensor)
					column.DimensionZ = _ReadPositiveNullableInt(reader);
				_column.Add(column);
			}

			_stream = stream;
			_dataOffset = stream.Position;
		}

		int? _ReadPositiveNullableInt(BinaryReader reader)
		{
			var ret = reader.ReadInt32();
			if (ret < 0)
				return null;
			return ret;
		}

		public static DataTable Create(Stream dataStream, Stream indexStream)
		{
			var reader = new BinaryReader(indexStream);
			var rowCount = reader.ReadInt32();
			var numIndex = reader.ReadInt32();
			var index = new long[numIndex];
			for (var i = 0; i < numIndex; i++)
				index[i] = reader.ReadInt64();
			return new DataTable(dataStream, index, rowCount);
		}

		public static DataTable Create(Stream dataStream)
		{
			var temp = new DataTable(dataStream, new long[0], 0);
			var index = new List<long>();
			var rowCount = 0;

			index.Add(dataStream.Position);
			var reader = new BinaryReader(dataStream);
			while (dataStream.Position < dataStream.Length) {
				temp._SkipRow(reader);
				if (++rowCount % temp.BlockSize == 0)
					index.Add(dataStream.Position);
			}
			dataStream.Seek(0, SeekOrigin.Begin);
			return new DataTable(dataStream, index, rowCount);
		}

		public IReadOnlyList<IColumn> Columns => _column;
		public int RowCount => _rowCount;
		public int ColumnCount => _column.Count;
		public int BlockSize => _blockSize;

		public bool HasCategoricalData
		{
			get
			{
				return _column.Any(c => !c.IsContinuous && !c.IsTarget);
			}
		}

		public void WriteTo(Stream stream)
		{
			var writer = new DataTableWriter(Columns, stream);
			Process(writer);
			writer.Flush();
		}

		public void WriteIndexTo(Stream stream)
		{
			using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
				writer.Write(_rowCount);
				writer.Write(_index.Count);
				foreach (var item in _index)
					writer.Write(item);
			}
		}

		object _ReadColumn(Column column, BinaryReader reader)
		{
			var type = column.Type;
			switch (type) {
				case ColumnType.String:
					return reader.ReadString();
				case ColumnType.Double:
					return reader.ReadDouble();
				case ColumnType.Int:
					return reader.ReadInt32();
				case ColumnType.Float:
					return reader.ReadSingle();
				case ColumnType.Boolean:
					return reader.ReadBoolean();
				case ColumnType.Date:
					return new DateTime(reader.ReadInt64());
				case ColumnType.Long:
					return reader.ReadInt64();
				case ColumnType.Byte:
					return reader.ReadSByte();
				case ColumnType.IndexList:
					return IndexList.ReadFrom(reader);
				case ColumnType.WeightedIndexList:
					return WeightedIndexList.ReadFrom(reader);
				case ColumnType.Vector:
					return FloatVector.ReadFrom(reader);
				case ColumnType.Matrix:
					return FloatMatrix.ReadFrom(reader);
				case ColumnType.Tensor:
					return FloatTensor.ReadFrom(reader);
				default:
					return null;
			}
		}

		protected DataTableRow _ReadDataTableRow(BinaryReader reader)
		{
			object[] Read()
			{
				var row = new object[_column.Count];
				for (var j = 0; j < _column.Count; j++)
					row[j] = _ReadColumn(_column[j], reader);
				return row;
			}

			return new DataTableRow(this, Read(), _rowConverter);
		}

		protected void _SkipRow(BinaryReader reader)
		{
			foreach (Column v in _column)
				_ReadColumn(v, reader);
		}

		public void Process(IRowProcessor rowProcessor)
		{
			_Iterate((row, i) => rowProcessor.Process(row));
		}

		protected void _Iterate(Func<IRow, int, bool> callback)
		{
			lock (_mutex) {
				_stream.Seek(_dataOffset, SeekOrigin.Begin);
				var reader = new BinaryReader(_stream, Encoding.UTF8, true);
				int index = 0;
				while (_stream.Position < _stream.Length) {
					var row = _ReadDataTableRow(reader);
					row.Index = index;
					if (!callback(row, index++))
						break;
				}
			}
		}

		public IDataTableAnalysis GetAnalysis()
		{
			if (_analysis == null) {
				var analysis = new DataTableAnalysis(this);
				Process(analysis);
				_analysis = analysis;
			}
			return _analysis;
		}

		public IReadOnlyList<IRow> GetSlice(int offset, int count)
		{
			var ret = new List<IRow>();
			lock (_mutex) {
				var reader = new BinaryReader(_stream, Encoding.UTF8, true);
				_stream.Seek(_index[offset / _blockSize], SeekOrigin.Begin);

				// seek to offset
				for (var i = 0; i < offset % _blockSize; i++)
					_SkipRow(reader);

				// read the data
				for (var i = 0; i < count && _stream.Position < _stream.Length; i++) {
					var row = _ReadDataTableRow(reader);
					row.Index = offset + i;
					ret.Add(row);
				}
			}
			return ret;
		}

		public IRow GetRow(int rowIndex)
		{
			var block = rowIndex / _blockSize;
			var offset = rowIndex % _blockSize;

			lock (_mutex) {
				var reader = new BinaryReader(_stream, Encoding.UTF8, true);
				_stream.Seek(_index[block], SeekOrigin.Begin);
				for (var i = 0; i < offset; i++)
					_SkipRow(reader);
				var ret = _ReadDataTableRow(reader);
				ret.Index = rowIndex;
				return ret;
			}
		}

		public IReadOnlyList<IRow> GetRows(IEnumerable<int> rowIndex)
		{
			return GetRows(rowIndex.ToList());
		}

		public IReadOnlyList<IRow> GetRows(IReadOnlyList<int> rowIndex)
		{
			// split the queries into blocks
			var blockMatch = new Dictionary<int, HashSet<int>>();
			var filteredRowIndex = rowIndex.Where(r => r >= 0 && r < RowCount).ToList();
			foreach (var row in filteredRowIndex.OrderBy(r => r)) {
				var block = row / _blockSize;
				if (!blockMatch.TryGetValue(block, out var set))
					blockMatch.Add(block, set = new HashSet<int>());
				set.Add(row);
			}

			var rowTable = new Dictionary<int, IRow>();
			lock (_mutex) {
				var reader = new BinaryReader(_stream, Encoding.UTF8, true);
				foreach (var block in blockMatch.OrderBy(b => b.Key)) {
					_stream.Seek(_index[block.Key], SeekOrigin.Begin);
					var match = block.Value;

					for (int i = block.Key * _blockSize, len = i + _blockSize; i < len && _stream.Position < _stream.Length; i++) {
						if (match.Contains(i)) {
							var row = _ReadDataTableRow(reader);
							row.Index = i;
							rowTable.Add(i, row);
						} else
							_SkipRow(reader);
					}
				}
			}

			return filteredRowIndex.Select(ind => rowTable[ind]).ToList();
		}

		public (IDataTable Training, IDataTable Test) Split(int? randomSeed = null, double trainPercentage = 0.8, bool shuffle = true, Stream output1 = null, Stream output2 = null)
		{
			var input = Enumerable.Range(0, RowCount);
			if (shuffle)
				input = input.Shuffle(randomSeed);
			var final = input.ToList();
			int trainingCount = Convert.ToInt32(RowCount * trainPercentage);

			var writer1 = new DataTableWriter(Columns, output1);
			foreach (var row in GetRows(final.Take(trainingCount)))
				writer1.Process(row);

			var writer2 = new DataTableWriter(Columns, output2);
			foreach (var row in GetRows(final.Skip(trainingCount)))
				writer2.Process(row);

			return (writer1.GetDataTable(), writer2.GetDataTable());
		}

		public IDataTable Bag(int? count = null, Stream output = null, int? randomSeed = null)
		{
			var input = Enumerable.Range(0, RowCount).ToList().Bag(count ?? RowCount, randomSeed);
			var writer = new DataTableWriter(Columns, output);
			foreach (var row in GetRows(input))
				writer.Process(row);
			return writer.GetDataTable();
		}

		public IEnumerable<(IDataTable Training, IDataTable Validation)> Fold(int k, int? randomSeed = null, bool shuffle = true)
		{
			var input = Enumerable.Range(0, RowCount);
			if (shuffle)
				input = input.Shuffle(randomSeed);
			var final = input.ToList();
			var foldSize = final.Count / k;

			for (var i = 0; i < k; i++) {
				var trainingRows = final.Take(i * foldSize).Concat(final.Skip((i + 1) * foldSize));
				var validationRows = final.Skip(i * foldSize).Take(foldSize);

				var writer1 = new DataTableWriter(Columns, null);
				foreach (var row in GetRows(trainingRows))
					writer1.Process(row);

				var writer2 = new DataTableWriter(Columns, null);
				foreach (var row in GetRows(validationRows))
					writer2.Process(row);

				yield return (writer1.GetDataTable(), writer2.GetDataTable());
			}
		}

		public IReadOnlyList<T> GetColumn<T>(int columnIndex)
		{
			var ret = new T[RowCount];

			_Iterate((row, i) => {
				ret[i] = row.GetField<T>(columnIndex);
				return true;
			});

			return ret;
		}

		public IReadOnlyList<float[]> GetNumericColumns(IEnumerable<int> columns = null)
		{
			var columnTable = (columns ?? Enumerable.Range(0, ColumnCount)).ToDictionary(i => i, i => new float[RowCount]);

			_Iterate((row, i) => {
				foreach (var item in columnTable)
					item.Value[i] = row.GetField<float>(item.Key);
				return true;
			});

			return columnTable.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
		}

		public IReadOnlyList<float[]> GetNumericRows(IEnumerable<int> columns = null)
		{
			var columnList = new List<int>(columns ?? Enumerable.Range(0, ColumnCount));

			var ret = new List<float[]>();
			_Iterate((row, i) => {
				int index = 0;
				var buffer = new float[columnList.Count];
				foreach (var item in columnList)
					buffer[index++] = row.GetField<float>(item);
				ret.Add(buffer);
				return true;
			});

			return ret;
		}

		public IDataTable Normalise(NormalisationType normalisationType, Stream output = null, IEnumerable<int> columnIndices = null)
		{
			var normaliser = new DataTableNormaliser(this, normalisationType, output, columnIndices);
			Process(normaliser);
			return normaliser.GetDataTable();
		}

		public IDataTable Normalise(DataTableNormalisation normalisationModel, Stream output = null)
		{
			var normaliser = new DataTableNormaliser(this, output, normalisationModel);
			Process(normaliser);
			return normaliser.GetDataTable();
		}

		public DataTableNormalisation GetNormalisationModel(NormalisationType normalisationType, IEnumerable<int> columnIndices = null)
		{
			var normaliser = new DataTableNormaliser(this, normalisationType, null, columnIndices);
			return normaliser.GetNormalisationModel();
		}

		public IRow this[int index] => GetRow(index);

		public int TargetColumnIndex
		{
			get
			{
				for (int i = 0, len = _column.Count; i < len; i++) {
					if (_column[i].IsTarget)
						return i;
				}
				// default to the last column
				return _column.Count - 1;
			}
			set
			{
				for (int i = 0, len = _column.Count; i < len; i++)
					_column[i].IsTarget = (i == value);
			}
		}

		public IReadOnlyList<(IRow Row, string Classification)> Classify(IRowClassifier classifier, Action<float> progress = null)
		{
			var ret = new List<(IRow, string)>();
			float total = RowCount;
			_Iterate((row, i) => {
				var bestClassification = classifier.Classify(row).GetBestClassification();
				ret.Add((row, bestClassification));
				progress?.Invoke(i / total);
				return true;
			});
			return ret;
		}

		public IDataTable SelectColumns(IEnumerable<int> columns, Stream output = null)
		{
			return DataTableProjector.Project(this, columns, output);
		}

		public IDataTable Project(Func<IRow, IReadOnlyList<object>> mutator, Stream output = null)
		{
			var isFirst = true;
			var writer = new DataTableWriter(output);
			_Iterate((row, i) => {
				var mutatedRow = mutator(row);
				if (mutatedRow != null) {
					if (isFirst) {
						var index = 0;
						foreach (var item in mutatedRow) {
							var column = Columns[index];
							if (item == null)
								writer.AddColumn(column.Name, ColumnType.Null, column.IsTarget);
							else {
								var type = item.GetType();
								ColumnType columnType;
								if (type == typeof(string))
									columnType = ColumnType.String;
								else if (type == typeof(double))
									columnType = ColumnType.Double;
								else if (type == typeof(float))
									columnType = ColumnType.Float;
								else if (type == typeof(long))
									columnType = ColumnType.Long;
								else if (type == typeof(int))
									columnType = ColumnType.Int;
								else if (type == typeof(sbyte))
									columnType = ColumnType.Byte;
								else if (type == typeof(DateTime))
									columnType = ColumnType.Date;
								else if (type == typeof(bool))
									columnType = ColumnType.Boolean;
								else if (type == typeof(FloatVector))
									columnType = ColumnType.Vector;
								else if (type == typeof(FloatMatrix))
									columnType = ColumnType.Matrix;
								else if (type == typeof(FloatTensor))
									columnType = ColumnType.Tensor;
								else if (type == typeof(WeightedIndexList))
									columnType = ColumnType.WeightedIndexList;
								else if (type == typeof(IndexList))
									columnType = ColumnType.IndexList;
								else
									throw new FormatException();

								writer.AddColumn(column.Name, columnType, column.IsTarget);
							}
							++index;
						}
						isFirst = false;
					}
					writer.AddRow(new DataTableRow(this, mutatedRow, _rowConverter));
				}
				return true;
			});
			return writer.GetDataTable();
		}

		public IDataTable Filter(Func<IRow, bool> filter, Stream output = null)
		{
			var writer = new DataTableWriter(Columns, output);
			_Iterate((row, i) => {
				if (filter(row))
					writer.AddRow(row);
				return true;
			});
			return writer.GetDataTable();
		}

		public IReadOnlyList<(IDataTable Table, string Classification)> ConvertToBinaryClassification()
		{
			var analysis = GetAnalysis();
			return analysis[TargetColumnIndex].DistinctValues
				.Select(val => val.ToString())
				.Select(cls => (Project(r => {
					var row = new object[ColumnCount];
					for (var i = 0; i < ColumnCount; i++) {
						if (i == TargetColumnIndex)
							row[i] = r.GetField<string>(i) == cls;
						else
							row[i] = r.Data[i];
					}
					return row;
				}), cls))
				.ToList()
			;
		}

		public void ForEach(Action<IRow> callback)
		{
			_Iterate((row, i) => { callback(row); return true; });
		}

		public void ForEach(Action<IRow, int> callback)
		{
			_Iterate((row, i) => { callback(row, i); return true; });
		}

		public void ForEach(Func<IRow, bool> callback)
		{
			_Iterate((row, i) => callback(row));
		}

		public IReadOnlyList<T> Map<T>(Func<IRow, T> mutator)
		{
			var ret = new List<T>();
			_Iterate((row, i) => {
				ret.Add(mutator(row));
				return true;
			});
			return ret;
		}

		public IDataTableVectoriser GetVectoriser(bool useTargetColumnIndex = true)
		{
			return new DataTableVectoriser(this, useTargetColumnIndex);
		}

		public IDataTableVectoriser GetVectoriser(DataTableVectorisation model)
		{
			return new DataTableVectoriser(model);
		}

		public IDataTable CopyWithRows(IEnumerable<int> rowIndices, Stream output = null)
		{
			var writer = new DataTableWriter(_column, output);
			foreach (var rowIndex in rowIndices) {
				var row = GetRow(rowIndex);
				writer.AddRow(row);
			}
			return writer.GetDataTable();
		}

		public IDataTable Zip(IDataTable dataTable, Stream output = null)
		{
			var writer = new DataTableWriter(_column.Concat(dataTable.Columns), output);
			_Iterate((row, i) => {
				if (i >= dataTable.RowCount)
					return false;
				writer.AddRow(row.Data.Concat(dataTable.GetRow(i).Data).ToList());
				return true;
			});
			return writer.GetDataTable();
		}

		public float Reduce(Func<IRow, float, float> reducer, float initialValue = 0f)
		{
			var value = initialValue;
			_Iterate((row, i) => {
				value = reducer(row, value);
				return true;
			});
			return value;
		}

		public float Average(Func<IRow, float> reducer)
		{
			var total = Reduce((row, sum) => sum + reducer(row));
			return total / RowCount;
		}

		public IDataTable ChangeColumnTypes(Dictionary<int, IConvertToType> columnConversion, bool removeInvalidRows = false)
		{
			return Project(row => {
				var data = row.Data;
				var skipRow = false;
				object Validate((object convertedValue, bool wasSuccessful) conversion)
				{
					if (removeInvalidRows && !conversion.wasSuccessful)
						skipRow = true;
					return conversion.convertedValue;
				}
				var convertedRow = Enumerable.Range(0, ColumnCount)
					.Select(i => columnConversion.TryGetValue(i, out var converter)
						? Validate(converter.ConvertValue(data[i]))
						: data[i]
					)
					.ToList()
				;
				return skipRow
					? null
					: convertedRow
				;
			});
		}

		public IDataTable Summarise(int newRowCount, Stream output = null, ISummariseRows summariser = null)
		{
			if (summariser == null)
				summariser = new AverageSummariser(this, _rowConverter);

			var bundleSize = RowCount / newRowCount;
			var bundle = new List<IRow>();
			var index = 0;

			return Project(row => {
				bundle.Add(row);
				if (bundle.Count >= bundleSize) {
					var ret = summariser.Summarise(bundle);
					bundle.Clear();
					return ret.Data;
				}

				// handle the last bundle
				if (++index == RowCount && bundle.Any())
					return summariser.Summarise(bundle).Data;

				// otherwise write nothing
				return null;
			}, output);
		}

		public string XmlPreview
		{
			get
			{
				var rows = GetRows(Enumerable.Range(0, Math.Min(20, RowCount)));
				var ret = new StringBuilder();
				using (var writer = XmlWriter.Create(new StringWriter(ret))) {
					writer.WriteStartElement("table");
					writer.WriteAttributeString("row-count", RowCount.ToString());
					foreach (var column in Columns) {
						writer.WriteStartElement("column");
						writer.WriteAttributeString("type", column.Type.ToString());
						writer.WriteAttributeString("name", column.Name);
						writer.WriteAttributeString("num-distinct", column.NumDistinct.ToString());
						writer.WriteAttributeString("is-continuous", column.IsContinuous ? "y" : "n");
						if (column.IsTarget)
							writer.WriteAttributeString("classification-target", "y");
						writer.WriteEndElement();
					}
					foreach (var row in rows) {
						writer.WriteStartElement("row");
						var index = 0;
						foreach (var val in row.Data) {
							writer.WriteStartElement("item");
							if (val == null)
								writer.WriteString("(null)");
							else {
								var type = Columns[index].Type;
								if (type == ColumnType.Vector)
									writer.WriteRaw(((FloatVector)val).Xml);
								else if (type == ColumnType.Matrix)
									writer.WriteRaw(((FloatMatrix)val).Xml);
								else if (type == ColumnType.Tensor)
									writer.WriteRaw(((FloatTensor)val).Xml);
								else if (type == ColumnType.IndexList)
									writer.WriteRaw(((IndexList)val).Xml);
								else if (type == ColumnType.WeightedIndexList)
									writer.WriteRaw(((WeightedIndexList)val).Xml);
								else
									writer.WriteString(val.ToString());
							}
							writer.WriteEndElement();
							++index;
						}
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}
				return ret.ToString();
			}
		}
	}
}
