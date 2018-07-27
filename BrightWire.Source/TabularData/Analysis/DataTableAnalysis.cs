using BrightWire.TabularData.Helper;
using System.Collections.Generic;
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
			//ColumnType.Boolean,
			ColumnType.Matrix,
			ColumnType.Null,
			ColumnType.Vector,
			ColumnType.Tensor
		};

        readonly IDataTable _table;
        readonly List<IRowProcessor> _column = new List<IRowProcessor>();

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
	        if (!_invalidColumnType.Contains(type))
	        {
		        if (ColumnTypeClassifier.IsNumeric(column))
			        _column.Add(new NumberCollector(index));
		        else if (type == ColumnType.String)
			        _column.Add(new StringCollector(index));
		        else if (type == ColumnType.IndexList || type == ColumnType.WeightedIndexList)
			        _column.Add(new IndexCollector(index));
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

        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                var table = ColumnInfo.ToDictionary(c => c.ColumnIndex, c => c);

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
                        
                        if(table.TryGetValue(columnIndex++, out IColumnInfo columnInfo)) {
                            writer.WriteAttributeString("column-index", columnInfo.ColumnIndex.ToString());
                            if (columnInfo.NumDistinct.HasValue)
                                writer.WriteAttributeString("distinct-value-count", columnInfo.NumDistinct.Value.ToString());

                            if (columnInfo is IStringColumnInfo stringColumn) {
                                writer.WriteAttributeString("min-length", stringColumn.MinLength.ToString());
                                writer.WriteAttributeString("max-length", stringColumn.MaxLength.ToString());
                                writer.WriteAttributeString("most-common-string", stringColumn.MostCommonString);
                            }

                            if (columnInfo is INumericColumnInfo numericColumn) {
                                writer.WriteAttributeString("min", numericColumn.Min.ToString());
                                writer.WriteAttributeString("max", numericColumn.Max.ToString());
                                writer.WriteAttributeString("mean", numericColumn.Mean.ToString());
                                writer.WriteAttributeString("l1", numericColumn.L1Norm.ToString());
                                writer.WriteAttributeString("l2", numericColumn.L2Norm.ToString());
                                if (numericColumn.StdDev.HasValue)
                                    writer.WriteAttributeString("std-dev", numericColumn.StdDev.Value.ToString());
                                if (numericColumn.Median.HasValue)
                                    writer.WriteAttributeString("median", numericColumn.Median.Value.ToString());
                                if (numericColumn.Mode.HasValue)
                                    writer.WriteAttributeString("mode", numericColumn.Mode.Value.ToString());
                            }

                            if (columnInfo is IIndexColumnInfo indexColumn) {
                                writer.WriteAttributeString("min-index", indexColumn.MinIndex.ToString());
                                writer.WriteAttributeString("max-index", indexColumn.MaxIndex.ToString());
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
