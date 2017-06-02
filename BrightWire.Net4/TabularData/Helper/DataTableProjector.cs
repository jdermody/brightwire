using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightWire.TabularData.Helper
{
    /// <summary>
    /// Projects a subset of data table rows into a new data table
    /// </summary>
    internal class DataTableProjector : IRowProcessor
    {
        readonly IRowProcessor _destination;
        readonly IReadOnlyList<int> _validColumn;
        readonly RowConverter _rowConverter = new RowConverter();

        class ProjectedRow : IRow
        {
            readonly int _index;
            readonly IReadOnlyList<object> _data;
            readonly RowConverter _rowConverter;

            public ProjectedRow(int index, IReadOnlyList<object> data, RowConverter rowConverter)
            {
                _data = data;
                _rowConverter = rowConverter;
            }

            public IReadOnlyList<object> Data => _data;
            public int Index => _index;
            public IHaveColumns Table => throw new NotImplementedException();

            public T GetField<T>(int index)
            {
                return _rowConverter.GetField<T>(_data, index);
            }

            public IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices)
            {
                var ret = new T[indices.Count];
                for (int i = 0, len = indices.Count; i < len; i++)
                    ret[i] = _rowConverter.GetField<T>(_data, i);
                return ret;
            }
        }


        public DataTableProjector(IRowProcessor destination, IEnumerable<int> columns)
        {
            _destination = destination;
            _validColumn = columns.ToList();
        }

        ProjectedRow _Project(IRow row)
        {
            var data = row.Data;
            var newRow = new List<object>();
            foreach (var item in _validColumn)
                newRow.Add(data[item]);
            return new ProjectedRow(row.Index, newRow, _rowConverter);
        }

        public bool Process(IRow row)
        {
            return _destination.Process(_Project(row));
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
