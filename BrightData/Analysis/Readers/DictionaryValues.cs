using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class DictionaryValues
    {
        readonly List<string> _table;

        internal DictionaryValues(IMetaData metaData)
        {
            var len = Consts.CategoryPrefix.Length;
            _table = metaData.GetStringsWithPrefix(Consts.CategoryPrefix)
                .Select(key => (Key: Int32.Parse(key[len..]), Value: metaData.Get<string>(key)))
                .OrderBy(d => d.Key)
                .Select(d => d.Value)
                .ToList();
        }

        public IEnumerable<string> GetValues(IEnumerable<int> dictionaryIndices) => dictionaryIndices.Select(i => _table[i]);

        public string GetValue(int dictionaryIndex) => _table[dictionaryIndex];
    }
}
