using BrightData;
using BrightData.Analysis;
using BrightTable;
using BrightTable.Helper;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian.Training
{
    /// <summary>
    /// Simple naive bayes trainer
    /// </summary>
    static class NaiveBayesTrainer
    {
        class FrequencyAnalysis
        {
            readonly Dictionary<uint, IDataAnalyser> _column = new Dictionary<uint, IDataAnalyser>();

            public FrequencyAnalysis(IDataTable table, uint ignoreColumnIndex)
            {
                uint index = 0;
                foreach (var columnType in table.ColumnTypes) {
                    if (index != ignoreColumnIndex) {
                        if (ColumnTypeClassifier.IsContinuous(columnType))
                            _column.Add(index, new NumericAnalyser());
                        else if (ColumnTypeClassifier.IsCategorical(columnType))
                            _column.Add(index, new FrequencyAnalyser<string>());
                    }
                    ++index;
                }
            }

            public void Process(object[] row)
            {
                foreach (var item in _column)
                    item.Value.AddObject(row[item.Key]);
                ++Total;
            }

            public IEnumerable<KeyValuePair<uint, IDataAnalyser>> Columns => _column;

            public ulong Total { get; private set; } = 0;
        }

        public static NaiveBayes Train(IDataTable table)
        {
            // analyse the table to get the set of class values
            var targetColumn = table.GetTargetColumnOrThrow();

            // analyse each row by its classification
            var rowsByClassification = new Dictionary<string, FrequencyAnalysis>();
            ulong rowCount = 0;
            table.ForEachRow((row, index) => {
                var target = row[targetColumn].ToString();
                if (!rowsByClassification.TryGetValue(target, out var analysis))
                    rowsByClassification.Add(target, analysis = new FrequencyAnalysis(table, targetColumn));
                analysis.Process(row);
                ++rowCount;
            });

            // create the per-class summaries from the frequency table
            var classList = new List<NaiveBayes.ClassSummary>();
            foreach (var classSummary in rowsByClassification) {
                var classLabel = classSummary.Key;
                var frequency = classSummary.Value;
                var columnList = new List<NaiveBayes.Column>();
                foreach (var column in frequency.Columns) {
                    if (column.Value is FrequencyAnalyser<string> categorical) {
                        var total = (double)categorical.Total;
                        if (total > 0) {
                            var list = new List<NaiveBayes.CategorialProbability>();
                            foreach (var item in categorical.ItemFrequency) {
                                var categoryProbability = item.Value / total;
                                list.Add(new NaiveBayes.CategorialProbability {
                                    Category = item.Key,
                                    LogProbability = Math.Log(categoryProbability),
                                    Probability = categoryProbability
                                });
                            }
                            columnList.Add(new NaiveBayes.Column {
                                Type = NaiveBayes.ColumnType.Categorical,
                                ColumnIndex = column.Key,
                                Probability = list
                            });
                        }
                    } else {
                        var continuous = column.Value as NumericAnalyser;
                        var variance = continuous?.Variance;
                        if (variance != null) {
                            var mean = continuous.Mean;
                            columnList.Add(new NaiveBayes.Column {
                                Type = NaiveBayes.ColumnType.ContinuousGaussian,
                                ColumnIndex = column.Key,
                                Mean = mean,
                                Variance = variance.Value
                            });
                        }
                    }
                }

                var probability = (double)classSummary.Value.Total / rowCount;
                classList.Add(new NaiveBayes.ClassSummary {
                    Label = classLabel,
                    ColumnSummary = columnList,
                    LogPrior = Math.Log(probability),
                    Prior = probability
                });
            }

            return new NaiveBayes {
                Class = classList
            };
        }
    }
}
