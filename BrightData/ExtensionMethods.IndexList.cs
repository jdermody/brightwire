using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Analysis;
using BrightData.Types;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="_"></param>
        /// <param name="reader">The binary reader</param>
        public static IndexList CreateIndexList(this BrightDataContext _, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var array = reader.BaseStream.ReadArray<uint>(len);
            return IndexList.Create(array);
        }

        /// <summary>
        /// Creates a weighted index list from a binary reader
        /// </summary>
        /// <param name="_"></param>
        /// <param name="reader">The binary reader</param>
        public static WeightedIndexList CreateWeightedIndexList(this BrightDataContext _, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = reader.BaseStream.ReadArray<WeightedIndexList.Item>(len);
            return WeightedIndexList.Create(ret);
        }

        /// <summary>
        /// Creates a weighted index list from indexed counts
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static WeightedIndexList ToWeightedIndexList(this IEnumerable<(uint Index, uint Count)> items) => WeightedIndexList.Create(items.Select(x => new WeightedIndexList.Item(x.Index, x.Count)));

        /// <summary>
        /// Converts the indices to an index list
        /// </summary>
        /// <param name="indices">Indices to convert</param>
        /// <returns></returns>
        public static IndexList ToIndexList(this IEnumerable<uint> indices) => IndexList.Create(indices);

        /// <summary>
        /// Converts the indices to a weighted index list
        /// </summary>
        /// <param name="indices">Indices to convert</param>
        /// <param name="weight">Weight to give each index</param>
        /// <returns></returns>
        public static WeightedIndexList ToWeightedIndexList(this IEnumerable<uint> indices, float weight = 1f) => WeightedIndexList.Create(indices.Select(x => new WeightedIndexList.Item(x, weight)));

        /// <summary>
        /// Converts the indexed classifications to weighted indexed classifications
        /// </summary>
        /// <param name="data"></param>
        /// <param name="groupByClassification">True to group by classification (i.e. convert the bag to a set)</param>
        public static WeightedIndexListWithLabel<T>[] ConvertToWeightedIndexList<T>(
            this IEnumerable<IndexListWithLabel<T>> data,
            bool groupByClassification
        )
        {
            if (groupByClassification)
            {
                return data.GroupBy(c => c.Label)
                    .Select(g => new WeightedIndexListWithLabel<T>(g.Key, WeightedIndexList.Create(g.SelectMany(d => d.Data.Indices)
                        .GroupBy(d => d)
                        .Select(g2 => new WeightedIndexList.Item(g2.Key, g2.Count()))
                        .ToArray()
                    )))
                    .ToArray()
                ;
            }
            return data
                .Select(d => new WeightedIndexListWithLabel<T>(d.Label, WeightedIndexList.Create(d.Data.Indices
                    .GroupBy(i => i)
                    .Select(g2 => new WeightedIndexList.Item(g2.Key, g2.Count()))
                    .ToArray()
                )))
                .ToArray()
            ;
        }

        /// <summary>
        /// Finds the greatest weight within the weighted index classification list
        /// </summary>
        /// <param name="data"></param>
        public static float GetMaxWeight<T>(this Span<WeightedIndexListWithLabel<T>> data)
        {
            var max = float.MinValue;
            foreach (ref var item in data) {
                foreach (ref readonly var index in item.Data.AsSpan()) {
                    if (index.Weight > max)
                        max = index.Weight;
                }
            }
            return max;
        }

        /// <summary>
        /// Find the greatest index within the weighted index classification list
        /// </summary>
        /// <param name="data"></param>
        public static uint GetMaxIndex<T>(this Span<WeightedIndexListWithLabel<T>> data)
        {
            uint max = 0;
            foreach (ref var item in data) {
                foreach (ref readonly var index in item.Data.AsSpan()) {
                    if (index.Index > max)
                        max = index.Index;
                }
            }
            return max;
        }

        /// <summary>
        /// Find the greatest index within the index classification list
        /// </summary>
        /// <param name="data"></param>
        public static uint GetMaxIndex<T>(this Span<IndexListWithLabel<T>> data)
        {
            uint max = 0;
            foreach (ref var item in data) {
                foreach (var index in item.Data.Indices) {
                    if (index > max)
                        max = index;
                }
            }
            return max;
        }

        /// <summary>
        /// Normalizes the weighted index classification list to fit between 0 and 1
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static WeightedIndexListWithLabel<T>[] Normalize<T>(this Span<WeightedIndexListWithLabel<T>> data)
        {
            var maxWeight = data.GetMaxWeight();
            var ret = new WeightedIndexListWithLabel<T>[data.Length];
            var index = 0;
            foreach(ref var item in data) {
                ret[index++] = item with { Data = WeightedIndexList.Create(
                    item.Data.Indices.Select(wi => new WeightedIndexList.Item(wi.Index, wi.Weight / maxWeight)).ToArray()
                )};
            }

            return ret;
        }

        static (Dictionary<uint, uint> IndexOccurrence, Dictionary<T, float> ClassificationSum) FindIndexOccurrence<T>(IEnumerable<WeightedIndexListWithLabel<T>> data) where T: notnull
        {
            var indexOccurrence = new Dictionary<uint, uint>();
            var classificationSum = new Dictionary<T, float>();

            // find the overall count of each index
            foreach (var classification in data.GroupBy(c => c.Label))
            {
                float sum = 0;
                foreach (var (_, weightedIndexList) in classification)
                {
                    foreach (ref readonly var index in weightedIndexList.ReadOnlySpan)
                    {
                        var key = index.Index;
                        if (indexOccurrence.TryGetValue(key, out var temp))
                            indexOccurrence[key] = temp + 1;
                        else
                            indexOccurrence.Add(key, 1);
                        sum += index.Weight;
                    }
                }
                classificationSum.Add(classification.Key, sum);
            }

            return (indexOccurrence, classificationSum);
        }

        /// <summary>
        /// Modifies the weights in the classification set based on relative corpus statistics to increase the weight of important words relative to each document
        /// https://en.wikipedia.org/wiki/Tf%E2%80%93idf
        /// </summary>
        /// <returns>A newly weighted classification set</returns>
        public static WeightedIndexListWithLabel<T>[] TfIdf<T>(this IReadOnlyCollection<WeightedIndexListWithLabel<T>> data) where T: notnull
        {
            int len = data.Count, i = 0;
            var ret = new WeightedIndexListWithLabel<T>[len];

            var (indexOccurrence, classificationSum) = FindIndexOccurrence(data);
            var numDocs = (float)len;

            // calculate tf-idf for each document
            foreach (var (label, weightedIndexList) in data) {
                var totalWords = classificationSum[label];
                var classificationIndex = new List<WeightedIndexList.Item>();
                foreach (ref readonly var item in weightedIndexList.ReadOnlySpan) {
                    var index = item.Index;
                    var tf = item.Weight / totalWords;
                    var docsWithTerm = (float)indexOccurrence[index];
                    var idf = MathF.Log(numDocs / (docsWithTerm + 1f)) + 1f;
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.Item(index, score));
                }

                ret[i++] = new(label, WeightedIndexList.Create(classificationIndex.ToArray()));
            }

            return ret;
        }

        /// <summary>
        /// Okapi B525+ modifies the weights in the classification set based on relative corpus statistics to increase the weight of important words relative to each document
        /// https://en.wikipedia.org/wiki/Okapi_BM25
        /// </summary>
        /// <returns>Newly weighted classification set</returns>
        public static WeightedIndexListWithLabel<T>[] Bm25Plus<T>(this IReadOnlyCollection<WeightedIndexListWithLabel<T>> data, float k = 1.2f, float b = 0.75f, float d = 1f) where T : notnull
        {
            int len = data.Count, i = 0;
            var ret = new WeightedIndexListWithLabel<T>[len];

            var (indexOccurrence, classificationSum) = FindIndexOccurrence(data);
            var averageDocumentWeight = classificationSum.Average(doc => doc.Value);
            var numDocs = (float)len;
                
            // calculate bm25f score for each document
            foreach (var (label, weightedIndexList) in data) {
                var documentWeight = classificationSum[label] / averageDocumentWeight;
                var classificationIndex = new List<WeightedIndexList.Item>();
                foreach (ref readonly var item in weightedIndexList.ReadOnlySpan) {
                    var index = item.Index;
                    var tf = (item.Weight * (k+1)) / ((item.Weight + k) * (1 - b + (b * documentWeight))) + d;
                    var docsWithTerm = (float)indexOccurrence[index];
                    var idf = MathF.Log((numDocs - docsWithTerm + 0.5f) / (0.5f + docsWithTerm) + 1);
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.Item(index, score));
                }

                ret[i++] = new(label, WeightedIndexList.Create(classificationIndex.ToArray()));
            }

            return ret;
        }

        public static (WeightedIndexListWithLabel<T>[], NormalisationModel Model) Normalize<T>(this IReadOnlyCollection<WeightedIndexListWithLabel<T>> data, NormalizationType normalizationType)
        {
            var analysis = new NumericAnalyser<float>();
            foreach (var item in data) {
                foreach(var value in item.Data)
                    analysis.Add(value.Weight);
            }

            var model = new NormalisationModel(normalizationType, analysis.GetMetaData());
            int len = data.Count, i = 0;
            var ret = new WeightedIndexListWithLabel<T>[len];
            foreach (var (label, weightedIndexList) in data)
                ret[i++] = new(label, weightedIndexList.Normalize(model));
            return (ret, model);
        }
    }
}
