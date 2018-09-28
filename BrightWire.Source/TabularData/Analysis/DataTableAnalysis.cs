using System;
using BrightWire.TabularData.Helper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightWire.TabularData.Analysis
{
    /// <summary>
    /// Collects meta data about data tables
    /// </summary>
    internal class DataTableAnalysis : IRowProcessor, IDataTableAnalysis
    {
		static readonly HashSet<ColumnType> _invalidColumnType = new HashSet<ColumnType>
		{
			ColumnType.Date,
			ColumnType.Null,
		};

        readonly IDataTable _table;
        readonly List<IRowProcessor> _column = new List<IRowProcessor>();
	    readonly List<IDataTableColumnFrequency> _columnFrequency = new List<IDataTableColumnFrequency>();

        public DataTableAnalysis(IDataTable table)
        {
            _table = table;

            int index = 0;
            foreach (var item in table.Columns)
                _Add(item, index++);
        }

        public DataTableAnalysis(IDataTable table, int columnIndex)
        {
            _table = table;

            var item = table.Columns[columnIndex];
            _Add(item, columnIndex);
        }

        void _Add(IColumn column, int index)
        {
            var type = column.Type;
	        if (!_invalidColumnType.Contains(type)) {
		        if (ColumnTypeClassifier.IsNumeric(column))
			        _column.Add(new NumberCollector(index));
		        else if (type == ColumnType.String)
			        _column.Add(new StringCollector(index));
		        else if (type == ColumnType.IndexList || type == ColumnType.WeightedIndexList)
			        _column.Add(new IndexCollector(index));
				else if (type == ColumnType.Date)
			        _column.Add(new DateCollector(index));
				else if (type == ColumnType.Vector || type == ColumnType.Matrix || type == ColumnType.Tensor)
			        _column.Add(new DimensionCollector(index));
				else if(ColumnTypeClassifier.IsCategorical(column))
					_column.Add(new FrequencyCollector(index));
	        }
        }

        public bool Process(IRow row)
        {
            foreach (var item in _column)
                item.Process(row);
            return true;
        }

        public IEnumerable<IColumnInfo> ColumnInfo => _column.Cast<IColumnInfo>();
	    public IColumnInfo this[int columnIndex] => ColumnInfo.FirstOrDefault(c => c.ColumnIndex == columnIndex);
	    public IReadOnlyList<IDataTableColumnFrequency> ColumnFrequency => _columnFrequency;

	    public void CollectFrequencies(IDataTable dataTable)
	    {
		    var numBins = 10; // TODO: make proportional to size of data?
		    var collectors = new List<IRowProcessor>();

		    foreach (var column in ColumnInfo) {
			    bool shouldCollect = true;
			    IDataTableColumnFrequency columnFrequency = null;
			    if (column is NumberCollector numeric) {
				    if (column.NumDistinct > numBins)
					    columnFrequency = new BinnedFrequencyCollector(column.ColumnIndex, numeric.Min, numeric.Max, numBins);
				    else
					    columnFrequency = new FrequencyCollector(column.ColumnIndex);
			    }else if(column is StringCollector strings && strings.NumDistinct <= numBins) 
					columnFrequency = new FrequencyCollector(column.ColumnIndex);
			    else if(column is DateCollector dates && dates.NumDistinct <= numBins)
				    columnFrequency = new FrequencyCollector(column.ColumnIndex);
				else if (column is FrequencyCollector existing) {
					columnFrequency = existing;
					shouldCollect = false;
			    }

			    if (columnFrequency != null) {
				    _columnFrequency.Add(columnFrequency);
				    if (shouldCollect)
					    collectors.Add((IRowProcessor)columnFrequency);
			    }
		    }

		    if (collectors.Any()) {
			    dataTable.ForEach(row => {
				    foreach (var collector in collectors)
					    collector.Process(row);
			    });
		    }
	    }

	    string _Write(double val) => val.ToString(CultureInfo.InvariantCulture);

        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                var table = ColumnInfo.ToDictionary(c => c.ColumnIndex, c => c);
	            var frequency = _columnFrequency.ToDictionary(c => c.ColumnIndex, c => c);

                using (var writer = XmlWriter.Create(new StringWriter(ret))) {
                    writer.WriteStartElement("table");
                    writer.WriteAttributeString("row-count", _table.RowCount.ToString());
                    int columnIndex = 0;
                    foreach (var column in _table.Columns) {
                        writer.WriteStartElement("column");
                        writer.WriteAttributeString("type", column.Type.ToString());
                        writer.WriteAttributeString("name", column.Name);
                        writer.WriteAttributeString("num-distinct", column.NumDistinct.ToString());
                        writer.WriteAttributeString("is-continuous", column.IsContinuous ? "y" : "n");
                        if (column.IsTarget)
                            writer.WriteAttributeString("classification-target", "y");
                        
                        if(table.TryGetValue(columnIndex++, out var columnInfo)) {
                            writer.WriteAttributeString("column-index", columnInfo.ColumnIndex.ToString());
                            if (columnInfo.NumDistinct.HasValue)
                                writer.WriteAttributeString("distinct-value-count", columnInfo.NumDistinct.Value.ToString());

                            if (columnInfo is IStringColumnInfo stringColumn) {
                                writer.WriteAttributeString("min-length", stringColumn.MinLength.ToString());
                                writer.WriteAttributeString("max-length", stringColumn.MaxLength.ToString());
                                writer.WriteAttributeString("most-common-string", stringColumn.MostCommonString);
                            }

                            if (columnInfo is INumericColumnInfo numericColumn) {
                                writer.WriteAttributeString("min", _Write(numericColumn.Min));
                                writer.WriteAttributeString("max", _Write(numericColumn.Max));
                                writer.WriteAttributeString("mean", _Write(numericColumn.Mean));
                                writer.WriteAttributeString("l1", _Write(numericColumn.L1Norm));
                                writer.WriteAttributeString("l2", _Write(numericColumn.L2Norm));
                                if (numericColumn.StdDev.HasValue)
                                    writer.WriteAttributeString("std-dev", _Write(numericColumn.StdDev.Value));
                                if (numericColumn.Median.HasValue)
                                    writer.WriteAttributeString("median", _Write(numericColumn.Median.Value));
                                if (numericColumn.Mode.HasValue)
                                    writer.WriteAttributeString("mode", _Write(numericColumn.Mode.Value));
                            }

                            if (columnInfo is IIndexColumnInfo indexColumn) {
                                writer.WriteAttributeString("min-index", indexColumn.MinIndex.ToString());
                                writer.WriteAttributeString("max-index", indexColumn.MaxIndex.ToString());
                            }

	                        if (columnInfo is IDateColumnInfo dateColumn) {
								if(dateColumn.MinDate.HasValue)
									writer.WriteAttributeString("min-date", dateColumn.MinDate.Value.ToString("s"));
								if(dateColumn.MaxDate.HasValue)
									writer.WriteAttributeString("max-date", dateColumn.MaxDate.Value.ToString("s"));
	                        }

	                        if (columnInfo is IDimensionsColumnInfo tensorColumn) {
		                        if(tensorColumn.XDimension.HasValue)
			                        writer.WriteAttributeString("x", tensorColumn.XDimension.Value.ToString());
		                        if(tensorColumn.YDimension.HasValue)
			                        writer.WriteAttributeString("y", tensorColumn.YDimension.Value.ToString());
		                        if(tensorColumn.ZDimension.HasValue)
			                        writer.WriteAttributeString("z", tensorColumn.ZDimension.Value.ToString());
	                        }

	                        if (frequency.TryGetValue(columnInfo.ColumnIndex, out var columnFrequency)) {
		                        double total = _table.RowCount;
		                        if (columnFrequency.CategoricalFrequency != null) {
			                        foreach (var item in columnFrequency.CategoricalFrequency.OrderByDescending(d => d.Count)) {
				                        writer.WriteStartElement("frequency");
				                        writer.WriteAttributeString("class", item.Category);
				                        writer.WriteString(_Write(item.Count / total));
				                        writer.WriteEndElement();
			                        }
		                        }else if (columnFrequency.ContinuousFrequency != null) {
			                        foreach (var item in columnFrequency.ContinuousFrequency) {
				                        writer.WriteStartElement("frequency-range");
				                        writer.WriteAttributeString("start", double.IsNegativeInfinity(item.Start) ? "-∞" : _Write(item.Start));
				                        writer.WriteAttributeString("end", double.IsPositiveInfinity(item.End) ? "∞" : _Write(item.End));
				                        writer.WriteString(_Write(item.Count / total));
				                        writer.WriteEndElement();
			                        }
		                        }
	                        }
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
