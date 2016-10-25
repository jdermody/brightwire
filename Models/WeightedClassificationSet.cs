using BrightWire.Connectionist.Helper;
using BrightWire.Models.Simple;
using BrightWire.TabularData;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class WeightedClassificationSet
    {
        [ProtoContract]
        public class WeightedIndex
        {
            [ProtoMember(1)]
            public uint Index { get; set; }

            [ProtoMember(2)]
            public float Weight { get; set; }
        }

        [ProtoContract]
        public class Classification
        {
            [ProtoMember(1)]
            public string Name { get; set; }

            [ProtoMember(2)]
            public WeightedIndex[] Data { get; set; }
        }

        [ProtoMember(1)]
        public Classification[] Classifications { get; set; }

        public uint GetMaximumIndex()
        {
            return Classifications.Select(d => d.Data.Max(d2 => d2.Index)).Max();
        }

        public float GetMaximumWeight()
        {
            return Classifications.Select(d => d.Data.Max(d2 => d2.Weight)).Max();
        }

        public Dictionary<string, uint> GetClassifications()
        {
            uint temp;
            var ret = new Dictionary<string, uint>();
            foreach(var item in Classifications) {
                if(!ret.TryGetValue(item.Name, out temp))
                    ret.Add(item.Name, temp = (uint)ret.Count);
            }
            return ret;
        }

        public ITrainingDataProvider CreateTrainingDataProvider(ILinearAlgebraProvider lap, uint maxIndex)
        {
            return new WeightedClassificationSetDataProvider(lap, this, maxIndex);
        }

        public IReadOnlyList<VectorClassification> Encode(bool normalise = true)
        {
            float maxWeight = 0;
            var max = GetMaximumIndex();
            var data = new List<Tuple<float[], string>>();

            foreach (var item in Classifications) {
                var vector = new float[max + 1];
                foreach (var index in item.Data) {
                    var weight = index.Weight;
                    if (weight > maxWeight)
                        maxWeight = weight;
                    vector[index.Index] = weight;
                }
                data.Add(Tuple.Create(vector, item.Name));
            }
            if (normalise) {
                foreach (var item in data) {
                    for (var i = 0; i <= max; i++)
                        item.Item1[i] /= maxWeight;
                }
            }
            return data.Select(d => new VectorClassification(d.Item1, d.Item2)).ToList();
        }

        public IDataTable ConvertToTable(Stream stream = null)
        {
            var max = GetMaximumIndex();
            var dataTable = new DataTableBuilder();
            for (var i = 0; i < max; i++)
                dataTable.AddColumn(ColumnType.Float, "term " + i.ToString());
            dataTable.AddColumn(ColumnType.String, "classification", true);

            foreach (var item in Classifications) {
                var data = new object[max + 1];
                for (var i = 0; i < max; i++)
                    data[i] = (float)0;
                foreach (var index in item.Data)
                    data[index.Index] = index.Weight;
                data[max] = item.Name;
                dataTable.AddRow(data);
            }

            return dataTable.Build(stream);
        }

        public WeightedClassificationSet Normalise()
        {
            var maxWeight = GetMaximumWeight();
            if (maxWeight == 0)
                throw new DivideByZeroException();

            return new WeightedClassificationSet {
                Classifications = Classifications.Select(c => new Classification {
                    Name = c.Name,
                    Data = c.Data.Select(d => new WeightedIndex {
                        Index = d.Index,
                        Weight = d.Weight / maxWeight
                    }).ToArray()
                }).ToArray()
            };
        }

        /// <summary>
        /// Modifies the weights in the classification set based on relative corpus statistics to increase the weight of important words relative to each document
        /// https://en.wikipedia.org/wiki/Tf%E2%80%93idf
        /// </summary>
        /// <returns>A new weighted classification set</returns>
        public WeightedClassificationSet TFIDF()
        {
            uint temp;
            var indexOccurence = new Dictionary<uint, uint>();
            var classificationSum = new Dictionary<string, double>();

            // find the overall count of each index
            foreach (var classification in Classifications.GroupBy(c => c.Name)) {
                double sum = 0;
                foreach (var item in classification) {
                    foreach (var index in item.Data) {
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
            var numDocs = (double)Classifications.Length;
            var ret = new List<Classification>();
            foreach (var classification in Classifications) {
                var totalWords = classificationSum[classification.Name];
                var classificationIndex = new List<WeightedIndex>();
                foreach (var item in classification.Data) {
                    var index = item.Index;
                    var tf = item.Weight / totalWords;
                    var docsWithTerm = (double)indexOccurence[index];
                    var idf = Math.Log(numDocs / (1.0 + docsWithTerm));
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndex {
                        Index = index,
                        Weight = Convert.ToSingle(score)
                    });
                }
                ret.Add(new Classification {
                    Name = classification.Name,
                    Data = classificationIndex.ToArray()
                });
            }
            return new WeightedClassificationSet {
                Classifications = ret.ToArray()
            };
        }
    }
}
