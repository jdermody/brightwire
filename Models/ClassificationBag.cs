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
    public class ClassificationBag
    {
        [ProtoContract]
        public class Classification
        {
            [ProtoMember(1)]
            public string Name { get; set; }

            [ProtoMember(2)]
            public uint[] Data { get; set; }
        }

        [ProtoMember(1)]
        public Classification[] Classifications { get; set; }

        /// <summary>
        /// Evaluates the classifier against each labelled classification
        /// </summary>
        /// <param name="classifier">The classifier to evaluate</param>
        /// <returns></returns>
        public IReadOnlyList<ClassificationResult> Classify(IIndexBasedClassifier classifier)
        {
            return Classifications
                .Select(d => new ClassificationResult(classifier.Classify(d.Data).First(), d.Name))
                .ToList()
            ;
        }

        public WeightedClassificationSet ConvertToSet(bool groupByClassification)
        {
            if (groupByClassification) {
                return new WeightedClassificationSet {
                    Classifications = Classifications
                    .GroupBy(c => c.Name)
                        .Select(g => new WeightedClassificationSet.Classification {
                            Name = g.Key,
                            Data = g.SelectMany(d => d.Data)
                                .GroupBy(d => d)
                                .Select(g2 => new WeightedClassificationSet.WeightedIndex {
                                    Index = g2.Key,
                                    Weight = g2.Count()
                                })
                                .ToArray()
                        })
                        .ToArray()
                };
            }else {
                return new WeightedClassificationSet {
                    Classifications = Classifications
                        .Select(c => new WeightedClassificationSet.Classification {
                            Name = c.Name,
                            Data = c.Data
                                .GroupBy(d => d)
                                .Select(g2 => new WeightedClassificationSet.WeightedIndex {
                                    Index = g2.Key,
                                    Weight = g2.Count()
                                })
                                .ToArray()
                        })
                        .ToArray()
                };
            }
        }
    }
}
