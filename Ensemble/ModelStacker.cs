using BrightWire.Models.Output;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.TabularData;
using System.IO;

namespace BrightWire.Ensemble
{
    /// <summary>
    /// Helps to create a new data table that holds the output from previous classifiers
    /// </summary>
    public class ModelStacker
    {
        readonly List<Tuple<float[], string>> _trainingData = new List<Tuple<float[], string>>();

        /// <summary>
        /// Converts the list of weighted classification outputs to a single vector
        /// </summary>
        /// <param name="row">The list of weighted classification outputs</param>
        public float[] Vectorise(IReadOnlyList<IReadOnlyList<WeightedClassification>> row)
        {
            List<float> temp;
            var table = new Dictionary<string, List<float>>();
            foreach(var group in row) {
                foreach(var item in group) {
                    if (!table.TryGetValue(item.Classification, out temp))
                        table.Add(item.Classification, temp = new List<float>());
                    temp.Add(item.Weight);
                }
            }
            return table
                .OrderBy(g => g.Key)
                .SelectMany(g => g.Value)
                .ToArray()
            ;
        }

        /// <summary>
        /// Adds a list of weighted classification outputs
        /// </summary>
        /// <param name="row">The list of weighted classification outputs</param>
        /// <param name="classification">The expected classification</param>
        public void Add(IReadOnlyList<IReadOnlyList<WeightedClassification>> row, string classification)
        {
            _trainingData.Add(Tuple.Create(Vectorise(row), classification));
        }

        /// <summary>
        /// Creates a new data table with the vectorised weighted classification outputs linked with each row's classification
        /// </summary>
        /// <param name="output">The output stream to write the table to (optional)</param>
        public IDataTable GetTable(Stream output = null)
        {
            if(_trainingData.Any()) {
                var fieldCount = _trainingData.First().Item1.Length;
                var builder = new DataTableBuilder();
                for (var i = 0; i < fieldCount; i++)
                    builder.AddColumn(ColumnType.Float, "v" + i);
                builder.AddColumn(ColumnType.String, "target", true);
                foreach(var item in _trainingData) {
                    var data = new object[fieldCount + 1];
                    for (var i = 0; i < fieldCount; i++)
                        data[i] = item.Item1[i];
                    data[fieldCount] = item.Item2;
                    builder.AddRow(data);
                }
                return builder.Build(output);
            }
            return null;
        }
    }
}
