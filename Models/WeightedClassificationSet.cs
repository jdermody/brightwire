using ProtoBuf;
using System;
using System.Collections.Generic;
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
            foreach (var classification in Classifications) {
                double sum = 0;
                foreach (var index in classification.Data) {
                    var key = index.Index;
                    if (indexOccurence.TryGetValue(key, out temp))
                        indexOccurence[key] = temp + 1;
                    else
                        indexOccurence.Add(key, 1);
                    sum += index.Weight;
                }
                classificationSum.Add(classification.Name, sum);
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
