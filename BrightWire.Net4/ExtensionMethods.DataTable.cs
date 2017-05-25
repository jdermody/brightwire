using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the underlying .net type associated with the column type
        /// </summary>
        /// <param name="type">The column type</param>
        public static Type GetColumnType(this ColumnType type)
        {
            switch (type) {
                case ColumnType.Boolean:
                    return typeof(bool);

                case ColumnType.Byte:
                    return typeof(byte);

                case ColumnType.Date:
                    return typeof(DateTime);

                case ColumnType.Double:
                    return typeof(double);

                case ColumnType.Float:
                    return typeof(float);

                case ColumnType.Int:
                    return typeof(int);

                case ColumnType.Long:
                    return typeof(long);

                case ColumnType.Null:
                    return null;

                case ColumnType.String:
                    return typeof(string);

                case ColumnType.IndexList:
                    return typeof(IndexList);

                case ColumnType.WeightedIndexList:
                    return typeof(WeightedIndexList);

                case ColumnType.Vector:
                    return typeof(FloatVector);

                case ColumnType.Matrix:
                    return typeof(FloatMatrix);

                case ColumnType.Tensor:
                    return typeof(FloatTensor);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Converts the indexed classifications to weighted indexed classifications
        /// </summary>
        /// <param name="groupByClassification">True to group by classification (i.e convert the bag to a set)</param>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> ConvertToWeightedIndexList(
            this IReadOnlyList<(string Label, IndexList Data)> data, 
            bool groupByClassification
        ){
            if (groupByClassification) {
                return data.GroupBy(c => c.Label)
                    .Select(g => (g.Key, new WeightedIndexList {
                        IndexList = g.SelectMany(d => d.Data.Index)
                            .GroupBy(d => d)
                            .Select(g2 => new WeightedIndexList.WeightedIndex {
                                Index = g2.Key,
                                Weight = g2.Count()
                            })
                            .ToArray()
                    }))
                    .ToArray()
                ;
            } else {
                return data
                    .Select(d => (d.Label, new WeightedIndexList {
                        IndexList = d.Data.Index
                            .GroupBy(i => i)
                            .Select(g2 => new WeightedIndexList.WeightedIndex {
                                Index = g2.Key,
                                Weight = g2.Count()
                            })
                            .ToArray()
                    }))
                    .ToArray()
                ;
            }
        }

        public static IDataTable ConvertToTable(this IReadOnlyList<(string Label, IndexList Data)> data)
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.IndexList, "Index");
            builder.AddColumn(ColumnType.String, "Label", true);

            foreach (var item in data)
                builder.Add(item.Data, item.Label);

            return builder.Build();
        }

        public static IDataTable ConvertToTable(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.WeightedIndexList, "Weighted Index");
            builder.AddColumn(ColumnType.String, "Label", true);

            foreach (var item in data)
                builder.Add(item.Data, item.Label);

            return builder.Build();
        }

        public static IDataTable ConvertToTable(this IReadOnlyList<(string Label, FloatVector Data)> data, bool preserveVectors)
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            if (preserveVectors) {
                builder.AddColumn(ColumnType.Vector, "Vector");
                builder.AddColumn(ColumnType.String, "Label", true);

                foreach (var item in data)
                    builder.Add(item.Data, item.Label);
            } else {
                var size = data.First().Data.Size;
                for (var i = 1; i <= size; i++)
                    builder.AddColumn(ColumnType.Float, "Value " + i);
                builder.AddColumn(ColumnType.String, "Label", true);

                foreach (var item in data) {
                    var vector = item.Data;
                    var row = new List<object>();
                    for (var i = 0; i < size; i++)
                        row.Add(vector.Data[i]);
                    row.Add(item.Label);
                    builder.Add(row);
                }
            }

            return builder.Build();
        }

        public static float GetMaxWeight(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.IndexList).Max(wi => wi.Weight);
        }

        public static uint GetMaxIndex(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.IndexList).Max(wi => wi.Index);
        }

        public static IReadOnlyList<(string Classification, FloatVector Data)> Vectorise(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            var size = data.GetMaxIndex() + 1;
            FloatVector _Create(WeightedIndexList weightedIndexList)
            {
                var ret = new float[size];
                foreach(var item in weightedIndexList.IndexList)
                    ret[item.Index] = item.Weight;
                return new FloatVector {
                    Data = ret
                };
            }
            return data.Select(r => (r.Label, _Create(r.Data))).ToList();
        }

        //public static IReadOnlyList<(FloatVector Label, FloatVector Data)> Vectorise(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        //{
        //    var size = data.GetMaxIndex() + 1;
        //    FloatVector _CreateData(WeightedIndexList weightedIndexList)
        //    {
        //        var ret = new float[size];
        //        foreach (var item in weightedIndexList.IndexList)
        //            ret[item.Index] = item.Weight;
        //        return new FloatVector {
        //            Data = ret
        //        };
        //    }
        //    var labelIndex = new HashSet<string>(data.Select(d => d.Label))
        //        .Select((d, i) => (d, i))
        //        .ToDictionary(d => d.Item1, d => d.Item2)
        //    ;
        //    FloatVector _CreateLabel(string label)
        //    {
        //        var ret = new float[labelIndex.Count];
        //        ret[labelIndex[label]] = 1f;
        //        return new FloatVector {
        //            Data = ret
        //        };
        //    }
        //    return data.Select(r => (_CreateLabel(r.Label), _CreateData(r.Data))).ToList();
        //}

        public static IReadOnlyList<(string Label, string Classification, float Score)> Classify(this IReadOnlyList<(string Label, IndexList Data)> data, IIndexListClassifier classifier)
        {
            var ret = new List<(string Label, string Classification, float Score)>();
            foreach (var item in data) {
                var classification = classifier.Classify(item.Data).GetBestClassification();
                ret.Add((item.Label, classification, item.Label == classification ? 1f : 0f));
            }
            return ret;
        }

        public static IReadOnlyList<(string Label, WeightedIndexList Data)> Normalise(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            var maxWeight = data.GetMaxWeight();
            return data.Select(r => (r.Label, new WeightedIndexList {
                IndexList = r.Data.IndexList.Select(wi => new WeightedIndexList.WeightedIndex {
                    Index = wi.Index,
                    Weight = wi.Weight / maxWeight
                }).ToArray()
            })).ToList();
        }

        /// <summary>
        /// Modifies the weights in the classification set based on relative corpus statistics to increase the weight of important words relative to each document
        /// https://en.wikipedia.org/wiki/Tf%E2%80%93idf
        /// </summary>
        /// <returns>A new weighted classification set</returns>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> TFIDF(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            uint temp;
            var indexOccurence = new Dictionary<uint, uint>();
            var classificationSum = new Dictionary<string, double>();

            // find the overall count of each index
            foreach (var classification in data.GroupBy(c => c.Label)) {
                double sum = 0;
                foreach (var item in classification) {
                    foreach (var index in item.Data.IndexList) {
                        var key = index.Index;
                        if (indexOccurence.TryGetValue(key, out temp))
                            indexOccurence[key] = temp + 1;
                        else
                            indexOccurence.Add(key, 1);
                        sum += index.Weight;
                    }
                }
                classificationSum.Add(classification.Key, sum);
            }

            // calculate tf-idf for each document
            var numDocs = (double)data.Count;
            var ret = new List<(string Label, WeightedIndexList Data)>();
            foreach (var classification in data) {
                var totalWords = classificationSum[classification.Label];
                var classificationIndex = new List<WeightedIndexList.WeightedIndex>();
                foreach (var item in classification.Data.IndexList) {
                    var index = item.Index;
                    var tf = item.Weight / totalWords;
                    var docsWithTerm = (double)indexOccurence[index];
                    var idf = Math.Log(numDocs / (1.0 + docsWithTerm));
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.WeightedIndex {
                        Index = index,
                        Weight = Convert.ToSingle(score)
                    });
                }
                ret.Add((classification.Label, new WeightedIndexList { IndexList = classificationIndex.ToArray() }));
            }
            return ret;
        }
    }
}
