using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Helper;
using BrightData.Types;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class WeightedIndexTests : UnitTestBase
    {
        [Fact]
        public void Inequality()
        {
            var first = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            var second = WeightedIndexList.Create((0, 1.1f), (1, 2.1f), (2, 3.1f));
            first.Equals(second).Should().BeFalse();
        }

        [Fact]
        public void Equality()
        {
            var first = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            var second = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            first.Equals(second).Should().BeTrue();
            var set = new HashSet<WeightedIndexList> {
                first, second
            };
            set.Count.Should().Be(1);
        }

        [Fact]
        public void MergeSum()
        {
            var first = WeightedIndexList.Create((0, 0.5f), (1, 0.5f));
            var second = WeightedIndexList.Create((1, 0.5f), (2, 0.5f));
            var merged = WeightedIndexList.Merge(new[] { first, second }, AggregationType.Sum);

            merged.Size.Should().Be(3);
            merged.Indices.Single(i => i.Index == 1).Weight.Should().Be(1.0f);
        }

        [Fact]
        public void MergeAverage()
        {
            var first = WeightedIndexList.Create((0, 0.5f), (1, 0.5f));
            var second = WeightedIndexList.Create((1, 0.5f), (2, 0.5f));
            var merged = WeightedIndexList.Merge(new[] { first, second }, AggregationType.Average);

            merged.Size.Should().Be(3);
            merged.Indices.Single(i => i.Index == 1).Weight.Should().Be(0.5f);
        }

        [Fact]
        public void MergeMax()
        {
            var first = WeightedIndexList.Create((0, 0.5f), (1, 1.5f));
            var second = WeightedIndexList.Create((1, 2.5f), (2, 0.5f));
            var merged = WeightedIndexList.Merge(new[] { first, second }, AggregationType.Max);

            merged.Size.Should().Be(3);
            merged.Indices.Single(i => i.Index == 1).Weight.Should().Be(2.5f);
        }

        [Fact]
        public void Serialisation()
        {
            var first = WeightedIndexList.Create((0, 0.5f), (1, 1.5f), (2, 2.5f));
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = _context.CreateWeightedIndexList(reader);
            first.Should().BeEquivalentTo(second);
        }

        [Fact]
        public void Serialisation2()
        {
            var first = WeightedIndexList.Create((0, 0.5f), (1, 1.5f), (2, 2.5f));
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = GenericActivator.CreateUninitialized<WeightedIndexList>();
            second.Initialize(_context, reader);
            first.Should().BeEquivalentTo(second);
        }

        [Fact]
        public void ToIndexList()
        {
            var first = WeightedIndexList.Create((0, 0.5f), (1, 1.5f), (2, 2.5f));
            first.AsIndexList().Should().BeEquivalentTo(IndexList.Create(0, 1, 2));
        }

        [Fact]
        public void Dot()
        {
            var first = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            var second = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            first.Dot(second).Should().Be(14);
        }

        [Fact]
        public void Magnitude()
        {
            var first = WeightedIndexList.Create((0, 2f));
            first.Magnitude.Should().Be(2);
        }

        [Fact]
        public void Max()
        {
            var first = WeightedIndexList.Create((0, 2f), (1, 3f));
            first.GetMaxWeight().Should().Be(3f);
        }

        [Fact]
        public void JaccardSimilarity()
        {
            var first = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            var second = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            var similarity = first.JaccardSimilarity(second);
            similarity.Should().Be(1);
        }

        [Fact]
        public void AsDense()
        {
            var first = WeightedIndexList.Create((0, 1f), (1, 2f), (2, 3f));
            var vector = first.AsDense();
            vector[0].Should().Be(1f);
            vector[1].Should().Be(2f);
            vector[2].Should().Be(3f);
        }

        static IEnumerable<uint> GetStringIndices(IEnumerable<string> words, Dictionary<string, uint> stringTable)
        {
            foreach (var word in words) {
                if (!stringTable.TryGetValue(word, out var index))
                    stringTable.Add(word, index = (uint)stringTable.Count);
                yield return index;
            }
        }

        WeightedIndexList GetWordCount(IEnumerable<string> words, Dictionary<string, uint> stringTable)
        {
            return WeightedIndexList.Create(GetStringIndices(words, stringTable).GroupBy(w => w).Select(g => new WeightedIndexList.Item(g.Key, g.Count())));
        }

        WeightedIndexListWithLabel<string>[] GetSampleDocuments()
        {
            var stringTable = new Dictionary<string, uint>();
            return new WeightedIndexListWithLabel<string>[] {
                new("Document 1", GetWordCount(new[] { "it", "is", "going", "to", "rain", "today" }, stringTable)),
                new("Document 2", GetWordCount(new[] { "today", "i", "am", "not", "going", "outside" }, stringTable)),
                new("Document 3", GetWordCount(new[] { "i", "am", "going", "to", "watch", "the", "season", "premiere" }, stringTable))
            };
        }

        [Fact]
        public void TfIdf()
        {
            var documents = GetSampleDocuments();
            var tfidf = documents.Tfidf();
            foreach (var (document, weightedTerms) in tfidf) {
                // check that each index is positive
                weightedTerms.Indices.Where(i => i.Weight <= 0).Should().BeEmpty();
            }
        }

        [Fact]
        public void Bm25Plus()
        {
            var documents = GetSampleDocuments();
            var tfidf = documents.Bm25Plus();
            foreach (var (document, weightedTerms) in tfidf) {
                // check that each index is positive
                weightedTerms.Indices.Where(i => i.Weight <= 0).Should().BeEmpty();
            }
        }
    }
}
