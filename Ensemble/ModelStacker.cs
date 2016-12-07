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

        public void Add(IReadOnlyList<IReadOnlyList<WeightedClassification>> row, string classification)
        {
            _trainingData.Add(Tuple.Create(Vectorise(row), classification));
        }

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
