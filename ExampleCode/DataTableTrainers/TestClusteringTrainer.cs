using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire.TrainingData.Helper;

namespace ExampleCode.DataTableTrainers
{
    class TestClusteringTrainer
    {
        public class AAAIDocument
        {
            /// <summary>
            /// Free text description of the document
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Free text; author-generated keywords
            /// </summary>
            public string[] Keyword { get; set; }

            /// <summary>
            /// Free text; author-selected, low-level keywords
            /// </summary>
            public string[] Topic { get; set; }

            /// <summary>
            /// Free text; paper abstracts
            /// </summary>
            public string Abstract { get; set; }

            /// <summary>
            /// Categorical; author-selected, high-level keyword(s)
            /// </summary>
            public string[] Group { get; set; }

            public (string Classification, WeightedIndexList Data) AsClassification(IBrightDataContext context, StringTableBuilder stringTable)
            {
                var weightedIndex = new List<WeightedIndexList.Item>();
                foreach (var item in Keyword)
                {
                    weightedIndex.Add(new WeightedIndexList.Item(stringTable.GetIndex(item), 1f));
                }
                foreach (var item in Topic)
                {
                    weightedIndex.Add(new WeightedIndexList.Item(stringTable.GetIndex(item), 1f));
                }
                return (Title, WeightedIndexList.Create(context, weightedIndex
                    .GroupBy(d => d.Index)
                    .Select(g => new WeightedIndexList.Item(g.Key, g.Sum(d => d.Weight)))
                    .ToArray()
                ));
            }
        }

        public TestClusteringTrainer(IReadOnlyList<AAAIDocument> documents)
        {

        }
    }
}
