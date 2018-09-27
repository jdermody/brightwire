using BrightWire.Models.Bayesian;
using BrightWire.TabularData.Analysis;
using BrightWire.TabularData.Helper;
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
	    class FrequencyAnalysis : IRowProcessor
	    {
		    readonly List<IRowProcessor> _column = new List<IRowProcessor>();

		    public FrequencyAnalysis(IDataTable table, int ignoreColumnIndex = -1)
		    {
			    int index = 0;
			    foreach (var item in table.Columns) {
				    if (index != ignoreColumnIndex) {
					    if (item.IsContinuous)
						    _column.Add(new NumberCollector(index));
					    else if (ColumnTypeClassifier.IsCategorical(item))
						    _column.Add(new FrequencyCollector(index));
				    }
				    ++index;
			    }
		    }

		    public bool Process(IRow row)
		    {
			    foreach (var item in _column)
				    item.Process(row);
			    return true;
		    }

		    public IEnumerable<IColumnInfo> ColumnInfo => _column.Cast<IColumnInfo>();
	    }

        public static NaiveBayes Train(IDataTable table)
        {
            // analyse the table to get the set of class values
            var classColumnIndex = table.TargetColumnIndex;
            var analysis = new DataTableAnalysis(table, classColumnIndex);
            table.Process(analysis);

            var classInfo = analysis.ColumnInfo.Single();
            if (classInfo.DistinctValues == null)
                throw new Exception("Too many class values");

            // analyse the data per class
            var classBasedFrequency = classInfo.DistinctValues.Select(cv => Tuple.Create<string, IRowProcessor>(cv.ToString(), new FrequencyAnalysis(table, classColumnIndex)));
            var frequencyAnalysis = new ClassificationBasedRowProcessor(classBasedFrequency, classColumnIndex);
            table.Process(frequencyAnalysis);

            // create the per-class summaries from the frequency table
            var classList = new List<NaiveBayes.ClassSummary>();
            foreach (var classSummary in frequencyAnalysis.All) {
                var classLabel = classSummary.Item1;
                var frequency = (FrequencyAnalysis)classSummary.Item2;
                var columnList = new List<NaiveBayes.Column>();
                foreach (var column in frequency.ColumnInfo) {
                    var continuous = column as NumberCollector;
                    if (column is FrequencyCollector categorical) {
                        var total = (double)categorical.Total;
                        if (total > 0) {
                            var list = new List<NaiveBayes.CategorialProbability>();
                            foreach (var item in categorical.Frequency) {
	                            var categoryProbability = item.Value / total;
                                list.Add(new NaiveBayes.CategorialProbability {
                                    Category = item.Key,
                                    LogProbability = Math.Log(categoryProbability),
									Probability = categoryProbability
                                });
                            }
                            columnList.Add(new NaiveBayes.Column {
								Type = NaiveBayes.ColumnType.Categorical,
                                ColumnIndex = categorical.ColumnIndex,
                                Probability = list
                            });
                        }
                    } else {
	                    var variance = continuous?.Variance;
	                    if (variance != null) {
		                    var mean = continuous.Mean;
		                    columnList.Add(new NaiveBayes.Column {
								Type = NaiveBayes.ColumnType.ContinuousGaussian,
			                    ColumnIndex = continuous.ColumnIndex,
			                    Mean = mean,
			                    Variance = variance.Value
		                    });
	                    }
                    }
                }

	            var probability = frequencyAnalysis.GetProbability(classLabel);
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
