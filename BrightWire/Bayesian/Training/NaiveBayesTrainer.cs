using BrightData;
using BrightData.Analysis;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightWire.Bayesian.Training
{
    /// <summary>
    /// Simple naive bayes trainer
    /// </summary>
    internal static class NaiveBayesTrainer
    {
        class FrequencyAnalysis
        {
            readonly Dictionary<uint, IDataAnalyser> _column = new();

            public FrequencyAnalysis(IDataTable table, uint ignoreColumnIndex)
            {
                uint index = 0;
                var metaData = table.ColumnMetaData;
                var columnTypes = table.ColumnTypes;

                for(int i = 0, len = columnTypes.Length; i < len; i++) {
                    if (index != ignoreColumnIndex) {
                        var columnClass = ColumnTypeClassifier.GetClass(columnTypes[i], metaData[i]);
                        if ((columnClass & ColumnClass.Categorical) != 0)
                            _column.Add(index, StaticAnalysers.CreateFrequencyAnalyser<string>());
                        else if((columnClass & ColumnClass.Numeric) != 0)
                            _column.Add(index, StaticAnalysers.CreateNumericAnalyser());
                        else
                            throw new NotImplementedException();
                    }
                    ++index;
                }
            }

            public void Process(TableRow row)
            {
                foreach (var (key, value) in _column)
                    value.AddObject(row[key]);
                ++Total;
            }

            public IEnumerable<KeyValuePair<uint, IDataAnalyser>> Columns => _column;

            public ulong Total { get; private set; } = 0;
        }

        public static async Task<NaiveBayes> Train(IDataTable table)
        {
            // analyse the table to get the set of class values
            var targetColumn = table.GetTargetColumnOrThrow();

            // analyse each row by its classification
            var rowsByClassification = new Dictionary<string, FrequencyAnalysis>();
            ulong rowCount = 0;
            await foreach(var row in table.EnumerateRows()) {
                var target = row[targetColumn].ToString();
                if (target != null) {
                    if (!rowsByClassification.TryGetValue(target, out var analysis))
                        rowsByClassification.Add(target, analysis = new FrequencyAnalysis(table, targetColumn));
                    analysis.Process(row);
                    ++rowCount;
                }
            }

            // create the per-class summaries from the frequency table
            var classList = new List<NaiveBayes.ClassSummary>();
            foreach (var (classLabel, frequency) in rowsByClassification) {
                var columnList = new List<NaiveBayes.Column>();
                foreach (var (key, dataAnalyser) in frequency.Columns) {
                    var metaData = new MetaData();
                    dataAnalyser.WriteTo(metaData);
                    if (metaData.IsNumeric()) {
                        var analysis = metaData.GetNumericAnalysis();
                        var variance = analysis.SampleVariance;
                        if (variance != null) {
                            var mean = analysis.Mean;
                            columnList.Add(new NaiveBayes.Column {
                                Type = NaiveBayes.ColumnType.ContinuousGaussian,
                                ColumnIndex = key,
                                Mean = mean,
                                Variance = variance.Value
                            });
                        }
                    }
                    else {
                        var analysis = metaData.GetFrequencyAnalysis();
                        var total = (double) analysis.Total;
                        if (total > 0) {
                            var list = new List<NaiveBayes.CategoricalProbability>();
                            foreach (var (label, value) in analysis.Frequency) {
                                var categoryProbability = value / total;
                                list.Add(new NaiveBayes.CategoricalProbability {
                                    Category = label,
                                    LogProbability = Math.Log(categoryProbability),
                                    Probability = categoryProbability
                                });
                            }

                            columnList.Add(new NaiveBayes.Column {
                                Type = NaiveBayes.ColumnType.Categorical,
                                ColumnIndex = key,
                                Probability = list.ToArray()
                            });
                        }
                    }
                }

                var probability = (double)frequency.Total / rowCount;
                classList.Add(new NaiveBayes.ClassSummary {
                    Label = classLabel,
                    ColumnSummary = columnList.ToArray(),
                    LogPrior = Math.Log(probability),
                    Prior = probability
                });
            }

            return new NaiveBayes {
                Class = classList.ToArray()
            };
        }
    }
}
