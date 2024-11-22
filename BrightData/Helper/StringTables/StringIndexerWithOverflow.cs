using System;
using System.Collections.Generic;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// A string indexer that provides an overflow buffer to index strings that are not represented in an underlying string indexer
    /// </summary>
    /// <param name="indexer">Underlying string indexer</param>
    public class StringIndexerWithOverflow(IIndexStrings indexer) : IIndexStrings
    {
        readonly DictionaryStringIndexer _overflow = new();

        /// <inheritdoc />
        public uint Size => indexer.Size + _overflow.Size;

        /// <inheritdoc />
        public uint GetIndex(ReadOnlySpan<char> str)
        {
            Span<char> temp = stackalloc char[str.Length];
            var size = str.Trim().ToLowerInvariant(temp);

            if (size >= 0) {
                var ret = indexer.GetIndex(temp);
                return ret != uint.MaxValue 
                    ? ret 
                    : _overflow.GetIndex(temp)
                ;
            }
            else {
                var str2 = str.Trim().ToString().ToLowerInvariant();
                var ret = indexer.GetIndex(str2);
                return ret != uint.MaxValue 
                    ? ret 
                    : _overflow.GetIndex(str2)
                ;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> OrderedStrings
        {
            get
            {
                foreach(var item in indexer.OrderedStrings)
                    yield return item;

                foreach (var item in _overflow.OrderedStrings)
                    yield return item;
            }
        }

        /// <summary>
        /// Additional strings that have been indexed
        /// </summary>
        public uint AdditionalStringCount => _overflow.Size;

        /// <summary>
        /// Additional strings that were not in the underlying string indexer
        /// </summary>
        public IEnumerable<string> AdditionalStrings => _overflow.OrderedStrings;
    }
}
