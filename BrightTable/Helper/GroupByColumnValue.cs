using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightTable.Helper
{
    class GroupByColumnValue
    {
        readonly uint[] _columnIndex;
        readonly Dictionary<string, List<uint>> _groups = new Dictionary<string, List<uint>>();

        public GroupByColumnValue(params uint[] columnIndex)
        {
            _columnIndex = columnIndex;
        }

        public void Analyse(IDataTable dataTable)
        {
            dataTable.ForEachRow((row, index) => {
                var str = String.Join("", _columnIndex.Select(i => row[i].ToString()));
                if (!_groups.TryGetValue(str, out var list))
                    _groups.Add(str, list = new List<uint>());
                list.Add(index);
            });
        }

        public IReadOnlyCollection<(string Item, IReadOnlyList<uint> Indices)> Groups => _groups
            .OrderBy(kv => kv.Key)
            .Select(kv => (Item: kv.Key, Indices: (IReadOnlyList<uint>)kv.Value.AsReadOnly()))
            .ToList();
    }
}
