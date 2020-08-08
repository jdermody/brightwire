using BrightWire.Bayesian;
using System.Collections.Generic;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A naive bayes model
    /// </summary>
    public class NaiveBayes
    {
        /// <summary>
        /// The type of data within the column
        /// </summary>
        public enum ColumnType
        {
            /// <summary>
            /// Continuous values
            /// </summary>
            ContinuousGaussian,

            /// <summary>
            /// Categorical values
            /// </summary>
            Categorical
        }

        /// <summary>
        /// A column within the naive bayes model
        /// </summary>
        public class Column
        {
            /// <summary>
            /// Index within the data set
            /// </summary>
            public uint ColumnIndex { get; set; }

	        /// <summary>
	        /// Type of column (categorical or continuous)
	        /// </summary>
	        public ColumnType Type { get; set; }

            /// <summary>
            /// The variance of the column values (continuous only)
            /// </summary>
            public double Variance { get; set; }

            /// <summary>
            /// The mean of the column values (continuous only)
            /// </summary>
            public double Mean { get; set; }

	        /// <summary>
	        /// The list of categories within the column and their probability (categorical only)
	        /// </summary>
	        public List<CategorialProbability> Probability { get; set; }
        }

        /// <summary>
        /// A category and its associated log probability
        /// </summary>
        public class CategorialProbability
        {
            /// <summary>
            /// The category label
            /// </summary>
            public string Category { get; set; }

            /// <summary>
            /// The natural log of the category's probability
            /// </summary>
            public double LogProbability { get; set; }

	        /// <summary>
	        /// The category's probability
	        /// </summary>
	        public double Probability { get; set; }
        }

        /// <summary>
        /// A classification and its associated data
        /// </summary>
        public class ClassSummary
        {
            /// <summary>
            /// The classification label
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// The natural log of the prior
            /// </summary>
            public double LogPrior { get; set; }

            /// <summary>
            /// The column data associated with this classification
            /// </summary>
            public List<Column> ColumnSummary { get; set; }

	        /// <summary>
	        /// The classification prior probability
	        /// </summary>
	        public double Prior { get; set; }
        }

        /// <summary>
        /// A list of possible classifications and their data
        /// </summary>
        public List<ClassSummary> Class { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <returns></returns>
        public IRowClassifier CreateClassifier()
        {
            return new NaiveBayesClassifier(this);
        }
    }
}
