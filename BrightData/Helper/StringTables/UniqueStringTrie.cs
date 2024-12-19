using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// An in-memory generic trie that maintains a prefix shortcut table based on usage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniqueIndexedStringTrie<T> : IHaveDataAsReadOnlyByteSpan
        where T: unmanaged, IComparable<T>
    {
        /// <summary>
        /// Represents a node within the trie
        /// </summary>
        /// <param name="ParentIndex">Parent index of this node</param>
        /// <param name="Character">Node data</param>
        /// <param name="Index">Index of following node</param>
        /// <param name="ValueIndex">Index (or max value if none) of the node's value</param>
        public readonly record struct NodeData(uint ParentIndex, T Character, uint Index, uint ValueIndex) : IComparable<NodeData>
        {
            /// <inheritdoc />
            public int CompareTo(NodeData other)
            {
                var ret = ParentIndex.CompareTo(other.ParentIndex);
                if (ret != 0)
                    return ret;

                ret = Character.CompareTo(other.Character);
                if (ret != 0)
                    return ret;

                ret = Index.CompareTo(other.Index);
                if (ret != 0)
                    return ret;

                return ValueIndex.CompareTo(other.ValueIndex);
            }
        }
        readonly record struct CharacterIndex(uint Index, T Character);

        const int PREFIX_SIZE = 5;
        [SkipLocalsInit, InlineArray(PREFIX_SIZE)]
        struct FixedSizePrefixArray : IComparable<FixedSizePrefixArray>
        {
            public T _ch0;

            [SkipLocalsInit]
            public FixedSizePrefixArray(ReadOnlySpan<T> str)
            {
                for(var i = 0; i < PREFIX_SIZE; i++)
                    this[i] = str[i];
            }

            public int CompareTo(FixedSizePrefixArray other)
            {
                ReadOnlySpan<T> data = this;
                ReadOnlySpan<T> data2 = other;
                return data.SequenceCompareTo(data2);
            }

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is FixedSizePrefixArray other) {
                    ReadOnlySpan<T> data = this;
                    ReadOnlySpan<T> data2 = other;
                    return data.SequenceEqual(data2);
                }
                return false;
            }

            [SkipLocalsInit]
            public override string ToString()
            {
                Span<T> buffer = stackalloc T[PREFIX_SIZE];
                for (var i = 0; i < PREFIX_SIZE; i++)
                    buffer[i] = this[i];
                return buffer.ToString();
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                foreach (var item in this)
                    hashCode.Add(item);
                return hashCode.ToHashCode();
            }
        }

        /// <summary>
        /// Builds a trie
        /// </summary>
        public class Builder
        {
            record Node(uint UniqueIndex)
            {
                readonly Dictionary<T, Node> _children = [];

                uint? ItemIndex { get; set; }

                public void Add(ReadOnlySpan<T> span, uint itemIndex, Func<uint> getNextIndex)
                {
                    if (span.Length == 0)
                        ItemIndex = itemIndex;
                    else {
                        var ch = span[0];
                        if(!_children.TryGetValue(ch, out var child))
                            _children.Add(ch, child = new(getNextIndex()));
                        child.Add(span[1..], itemIndex, getNextIndex);
                    }
                }

                public void WriteTo(uint parentIndex, IBuffer<NodeData> buffer)
                {
                    var span = buffer.GetSpan(_children.Count);
                    var index = 0;
                    var children = new List<Node>();
                    foreach (var (ch, child) in _children) {
                        span[index++] = new NodeData(parentIndex, ch, child.UniqueIndex, child.ItemIndex ?? uint.MaxValue);
                        children.Add(child);
                    }
                    buffer.Advance(_children.Count);

                    foreach(var child in children)
                        child.WriteTo(child.UniqueIndex, buffer);
                }
            }
            readonly Node _root;
            uint _nextIndex = 0;

            /// <summary>
            /// Creates an empty builder
            /// </summary>
            public Builder()
            {
                _root = new(GetNextIndex());
            }

            /// <summary>
            /// Adds a new string
            /// </summary>
            /// <param name="str">String to add</param>
            /// <param name="index">String index</param>
            public void Add(ReadOnlySpan<T> str, uint index)
            {
                _root.Add(str, index, GetNextIndex);
            }

            /// <summary>
            /// Builds a trie in memory
            /// </summary>
            /// <returns></returns>
            public UniqueIndexedStringTrie<T> Build()
            {
                // write and sort the data
                using var indexedCharacters = new ArrayPoolBufferWriter<NodeData>();
                _root.WriteTo(_root.UniqueIndex, indexedCharacters);
                var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(indexedCharacters.WrittenSpan), indexedCharacters.WrittenCount);
                span.Sort();
                return new(indexedCharacters.WrittenSpan.ToArray());
            }

            uint GetNextIndex() => _nextIndex++;
        }

        readonly ref struct CharacterComparator(in CharacterIndex data) : IComparable<NodeData>
        {
            readonly ref readonly CharacterIndex _data = ref data;

            public int CompareTo(NodeData other)
            {
                var ret = _data.Index.CompareTo(other.ParentIndex);
                if(ret != 0) 
                    return ret;
                return _data.Character.CompareTo(other.Character);
            }
        }
        readonly ref struct IndexComparator(uint Index) : IComparable<NodeData>
        {
            public int CompareTo(NodeData other)
            {
                return Index.CompareTo(other.ParentIndex);
            }
        }

        readonly ReadOnlyMemory<NodeData> _data;
        readonly Dictionary<FixedSizePrefixArray, int> _prefixes = [];
        static readonly NodeData _defaultRoot = new(uint.MaxValue, default, 0, uint.MaxValue);

        /// <summary>
        /// Creates a trie from node data
        /// </summary>
        /// <param name="data"></param>
        public UniqueIndexedStringTrie(ReadOnlyMemory<NodeData> data)
        {
            _data = data;
        }

        public static async Task<UniqueIndexedStringTrie<T>> Load(string filePath)
        {
            var data = await File.ReadAllBytesAsync(filePath);
            return new UniqueIndexedStringTrie<T>(data.AsMemory().Cast<byte, NodeData>());
        }

        /// <summary>
        /// Searches the trie for an index from the specified string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetIndex(ReadOnlySpan<T> str, out uint ret)
        {
            // prefix lookup
            FixedSizePrefixArray prefix = new();
            ref readonly var curr = ref _defaultRoot;
            var data = _data.Span;
            var hasPrefix = str.Length >= PREFIX_SIZE;
            if (hasPrefix) {
                prefix = new(str);
                if (_prefixes.TryGetValue(prefix, out var index)) {
                    curr = ref data[index];
                    str = str[PREFIX_SIZE..];
                    hasPrefix = false;
                }
            }
            
            var isValid = true;
            var count = 0;
            while (str.Length > 0) {
                var index = new CharacterIndex(curr.Index, str[0]);
                var characterIndex = data.BinarySearch(new CharacterComparator(in index));
                if (characterIndex < 0) {
                    isValid = false;
                    break;
                }

                curr = ref data[characterIndex];
                str = str[1..];
                if (hasPrefix && ++count == PREFIX_SIZE)
                    _prefixes.Add(prefix, characterIndex);
            }

            if (isValid && curr.ValueIndex != uint.MaxValue) {
                ret = curr.ValueIndex;
                return true;
            }

            ret = default;
            return false;
        }

        public IEnumerable<uint> Search(ReadOnlySpan<T> prefix)
        {
            var isValid = true;
            var data = _data.Span;
            ref readonly var curr = ref _defaultRoot;

            while (prefix.Length > 0) {
                var index = new CharacterIndex(curr.Index, prefix[0]);
                var characterIndex = data.BinarySearch(new CharacterComparator(in index));
                if (characterIndex < 0) {
                    isValid = false;
                    break;
                }

                curr = ref data[characterIndex];
                prefix = prefix[1..];
            }

            if (isValid) {
                var ret = new List<uint>();
                var indices = new Stack<uint>();
                indices.Push(curr.Index);
                if (curr.ValueIndex != uint.MaxValue)
                    ret.Add(curr.ValueIndex);

                while (indices.Count > 0) {
                    var comparator = new IndexComparator(indices.Pop());
                    foreach (ref readonly var item in data.MultiBinarySearchSpan(comparator)) {
                        if (item.ValueIndex != uint.MaxValue)
                            ret.Add(item.ValueIndex);
                        indices.Push(item.Index);
                    }
                }
                return ret;
            }
            return [];
        }

        public IReadOnlyList<(T[] Item, uint Index)> Complete(ReadOnlySpan<T> prefix)
        {
            var isValid = true;
            var data = _data.Span;
            ref readonly var curr = ref _defaultRoot;

            var prefixData = prefix.ToArray();
            while (prefix.Length > 0) {
                var index = new CharacterIndex(curr.Index, prefix[0]);
                var characterIndex = data.BinarySearch(new CharacterComparator(in index));
                if (characterIndex < 0) {
                    isValid = false;
                    break;
                }

                curr = ref data[characterIndex];
                prefix = prefix[1..];
            }

            var ret = new List<(T[] Item, uint Index)>();
            if (isValid) {
                var indices = new Stack<(uint Index, T[] Prefix)>();
                indices.Push((curr.Index, prefixData));
                if (curr.ValueIndex != uint.MaxValue)
                    ret.Add((prefixData, curr.ValueIndex));

                while (indices.Count > 0) {
                    var (nextIndex, str) = indices.Pop();
                    var comparator = new IndexComparator(nextIndex);
                    foreach (ref readonly var item in data.MultiBinarySearchSpan(comparator)) {
                        T[] nextData = [..str, item.Character];
                        indices.Push((item.Index, nextData));
                        if (item.ValueIndex != uint.MaxValue)
                            ret.Add((nextData, item.ValueIndex));
                    }
                }
                return ret;
            }
            return ret;
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => _data.Span.AsBytes();
    }
}
