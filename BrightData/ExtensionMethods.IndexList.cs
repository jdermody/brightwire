using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates an index list from indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indices">Indices</param>
        /// <returns></returns>
        public static IndexList CreateIndexList(this BrightDataContext context, params uint[] indices) => IndexList.Create(indices);

        /// <summary>
        /// Creates an index list from indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indices">Indices</param>
        /// <returns></returns>
        public static IndexList CreateIndexList(this BrightDataContext context, IEnumerable<uint> indices) => IndexList.Create(indices);

        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static IndexList CreateIndexList(this BrightDataContext context, BinaryReader reader)
        {
            var ret = new IndexList(Array.Empty<uint>());
            ret.Initialize(context, reader);
            return ret;
        }

        /// <summary>
        /// Creates a weighted index list from weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList CreateWeightedIndexList(this BrightDataContext context, params (uint Index, float Weight)[] indexList) => WeightedIndexList.Create(indexList);

        /// <summary>
        /// Creates a weighted index list from weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList CreateWeightedIndexList(this BrightDataContext context, IEnumerable<(uint Index, float Weight)> indexList) => WeightedIndexList.Create(indexList);

        /// <summary>
        /// Creates a weighted index list from weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList CreateWeightedIndexList(this BrightDataContext context, IEnumerable<WeightedIndexList.Item> indexList) => WeightedIndexList.Create(indexList);

        /// <summary>
        /// Creates a weighted index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static WeightedIndexList CreateWeightedIndexList(this BrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = reader.BaseStream.ReadArray<WeightedIndexList.Item>(len);
            return WeightedIndexList.Create(ret);
        }

        static BrightDataContext? GetContext<T, T2>(IReadOnlyList<(T, T2 Data)> list) where T2: IHaveBrightDataContext => list.Count == 0 
            ? null 
            : list[0].Data.Context
        ;

        /// <summary>
        /// Converts the indexed classifications to weighted indexed classifications
        /// </summary>
        /// <param name="data"></param>
        /// <param name="groupByClassification">True to group by classification (i.e convert the bag to a set)</param>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> ConvertToWeightedIndexList(
            this IReadOnlyList<(string Label, IndexList Data)> data,
            bool groupByClassification
        )
        {
            if (groupByClassification)
            {
                return data.GroupBy(c => c.Label)
                    .Select(g => (g.Key, WeightedIndexList.Create(g.SelectMany(d => d.Data.Indices)
                        .GroupBy(d => d)
                        .Select(g2 => new WeightedIndexList.Item(g2.Key, g2.Count()))
                        .ToArray()
                    )))
                    .ToArray()
                ;
            }
            return data
                .Select(d => (d.Label, WeightedIndexList.Create(d.Data.Indices
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
        public static float GetMaxWeight(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.Indices).Max(wi => wi.Weight);
        }

        /// <summary>
        /// Find the greatest index within the weighted index classification list
        /// </summary>
        /// <param name="data"></param>
        public static uint GetMaxIndex(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.Indices).Max(wi => wi.Index);
        }

        /// <summary>
        /// Find the greatest index within the index classification list
        /// </summary>
        /// <param name="data"></param>
        public static uint GetMaxIndex(this IReadOnlyList<(string Label, IndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.Indices).Max();
        }

        /// <summary>
        /// Normalizes the weighted index classification list to fit between 0 and 1
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> Normalize(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            var maxWeight = data.GetMaxWeight();

            return data.Select(r => (r.Label, WeightedIndexList.Create(
                r.Data.Indices.Select(wi => new WeightedIndexList.Item(wi.Index, wi.Weight / maxWeight)).ToArray()
            ))).ToList();
        }

        static (Dictionary<uint, uint> IndexOccurrence, Dictionary<T, float> ClassificationSum) FindIndexOccurrence<T>(IReadOnlyList<(T Label, WeightedIndexList Data)> data) where T: notnull
        {
            var indexOccurrence = new Dictionary<uint, uint>();
            var classificationSum = new Dictionary<T, float>();

            // find the overall count of each index
            foreach (var classification in data.GroupBy(c => c.Label))
            {
                float sum = 0;
                foreach (var (_, weightedIndexList) in classification)
                {
                    foreach (var index in weightedIndexList.Indices)
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
        public static IReadOnlyList<(T Label, WeightedIndexList Data)> Tfidf<T>(this IReadOnlyList<(T Label, WeightedIndexList Data)> data) where T: notnull
        {
            var ret = new List<(T Label, WeightedIndexList Data)>();

            var (indexOccurrence, classificationSum) = FindIndexOccurrence(data);
            var numDocs = (float)data.Count;

            // calculate tf-idf for each document
            foreach (var (label, weightedIndexList) in data) {
                var totalWords = classificationSum[label];
                var classificationIndex = new List<WeightedIndexList.Item>();
                foreach (var item in weightedIndexList.Indices) {
                    var index = item.Index;
                    var tf = item.Weight / totalWords;
                    var docsWithTerm = (float)indexOccurrence[index];
                    var idf = MathF.Log(numDocs / (docsWithTerm + 1f)) + 1f;
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.Item(index, score));
                }

                ret.Add((label, WeightedIndexList.Create(classificationIndex.ToArray())));
            }

            return ret;
        }

        /// <summary>
        /// Okapi B525+ modifies the weights in the classification set based on relative corpus statistics to increase the weight of important words relative to each document
        /// https://en.wikipedia.org/wiki/Okapi_BM25
        /// </summary>
        /// <returns>Newly weighted classification set</returns>
        public static IReadOnlyList<(T Label, WeightedIndexList Data)> Bm25Plus<T>(this IReadOnlyList<(T Label, WeightedIndexList Data)> data, float k = 1.2f, float b = 0.75f, float d = 1f) where T : notnull
        {
            var ret = new List<(T Label, WeightedIndexList Data)>();

            var (indexOccurrence, classificationSum) = FindIndexOccurrence(data);
            var averageDocumentWeight = classificationSum.Average(doc => doc.Value);
            var numDocs = (float)data.Count;
                
            // calculate bm25f score for each document
            foreach (var (label, weightedIndexList) in data) {
                var documentWeight = classificationSum[label] / averageDocumentWeight;
                var classificationIndex = new List<WeightedIndexList.Item>();
                foreach (var item in weightedIndexList.Indices) {
                    var index = item.Index;
                    var tf = (item.Weight * (k+1)) / ((item.Weight + k) * (1 - b + (b * documentWeight))) + d;
                    var docsWithTerm = (float)indexOccurrence[index];
                    var idf = MathF.Log((numDocs - docsWithTerm + 0.5f) / (0.5f + docsWithTerm) + 1);
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.Item(index, score));
                }

                ret.Add((label, WeightedIndexList.Create(classificationIndex.ToArray())));
            }

            return ret;
        }
    }
}
