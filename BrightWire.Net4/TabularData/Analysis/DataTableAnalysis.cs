using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.TabularData.Analysis
{
    internal class DataTableAnalysis : IRowProcessor, IDataTableAnalysis
    {
        readonly IDataTable _table;
        readonly List<IRowProcessor> _column = new List<IRowProcessor>();

        public DataTableAnalysis(IDataTable table)
        {
            _table = table;

            int index = 0;
            foreach (var item in table.Columns) {
                if (ColumnTypeClassifier.IsNumeric(item))
                    _column.Add(new NumberCollector(index));
                else if (item.Type == ColumnType.String)
                    _column.Add(new StringCollector(index));
                ++index;
            }
        }

        public DataTableAnalysis(IDataTable table, int columnIndex)
        {
            _table = table;

            var item = table.Columns[columnIndex];
            if (ColumnTypeClassifier.IsNumeric(item))
                _column.Add(new NumberCollector(columnIndex));
            else if (item.Type == ColumnType.String)
                _column.Add(new StringCollector(columnIndex));
        }

        public bool Process(IRow row)
        {
            foreach (var item in _column)
                item.Process(row);
            return true;
        }

        public IEnumerable<IColumnInfo> ColumnInfo { get { return _column.Cast<IColumnInfo>(); } }

        public IColumnInfo this[int columnIndex]
        {
            get
            {
                return _column
                    .Cast<IColumnInfo>()
                    .Where(c => c.ColumnIndex == columnIndex)
                    .FirstOrDefault()
                ;
            }
        }

        public string AsXml
        {
            get
            {
                IColumnInfo columnInfo;
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
                        
                        if(table.TryGetValue(columnIndex++, out columnInfo)) {
                            writer.WriteAttributeString("column-index", columnInfo.ColumnIndex.ToString());
                            if (columnInfo.NumDistinct.HasValue)
                                writer.WriteAttributeString("distinct-value-count", columnInfo.NumDistinct.Value.ToString());

                            var stringColumn = columnInfo as IStringColumnInfo;
                            if(stringColumn != null) {
                                writer.WriteAttributeString("min-length", stringColumn.MinLength.ToString());
                                writer.WriteAttributeString("max-length", stringColumn.MaxLength.ToString());
                                writer.WriteAttributeString("most-common-string", stringColumn.MostCommonString);
                            }

                            var numericColumn = columnInfo as INumericColumnInfo;
                            if(numericColumn != null) {
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
