using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    public class DataTableProjector : IRowProcessor
    {
        readonly IRowProcessor _destination;
        readonly IReadOnlyList<int> _validColumn;

        class ProjectedRow : IRow
        {
            readonly IReadOnlyList<object> _data;

            public ProjectedRow(IReadOnlyList<object> data)
            {
                _data = data;
            }

            public IReadOnlyList<object> Data
            {
                get
                {
                    return _data;
                }
            }

            public T GetField<T>(int index)
            {
                return DataTableRow.GetField<T>(_data, index);
            }
        }

        public DataTableProjector(IRowProcessor destination, IEnumerable<int> columns)
        {
            _destination = destination;
            _validColumn = columns.ToList();
        }

        public bool Process(IRow row)
        {
            var data = row.Data;
            var newRow = new List<object>();
            foreach(var item in _validColumn)
                newRow.Add(data[item]);
            return _destination.Process(new ProjectedRow(newRow));
        }

        public static IDataTable Project(IDataTable table, IEnumerable<int> columns, Stream output = null)
        {
            var validColumn = new HashSet<int>(columns);
            var writer = new DataTableWriter(table.Columns.Select((c, i) => Tuple.Create(c, i)).Where(c => validColumn.Contains(c.Item2)).Select(c => c.Item1), output);
            var projector = new DataTableProjector(writer, columns);
            table.Process(projector);
            return writer.GetDataTable();
        }

        public static IDataTable Project(IDataTable table, params int[] columns)
        {
            return Project(table, columns);
        }
    }
}
