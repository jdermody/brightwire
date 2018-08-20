using BrightWire.Bayesian;
using ProtoBuf;
using System.Collections.Generic;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A naive bayes model
    /// </summary>
    [ProtoContract]
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
        [ProtoContract]
        public class Column
        {
            /// <summary>
            /// Index within the data set
            /// </summary>
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

	        /// <summary>
	        /// Type of column (categorical or continuous)
	        /// </summary>
	        [ProtoMember(2)]
	        public ColumnType Type { get; set; }

            /// <summary>
            /// The variance of the column values (continuous only)
            /// </summary>
            [ProtoMember(3)]
            public double Variance { get; set; }

            /// <summary>
            /// The mean of the column values (continuous only)
            /// </summary>
            [ProtoMember(4)]
            public double Mean { get; set; }

	        /// <summary>
	        /// The list of categories within the column and their probability (categorical only)
	        /// </summary>
	        [ProtoMember(5)]
	        public List<CategorialProbability> Probability { get; set; }
        }

        /// <summary>
        /// A category and its associated log probability
        /// </summary>
        [ProtoContract]
        public class CategorialProbability
        {
            /// <summary>
            /// The category label
            /// </summary>
            [ProtoMember(1)]
            public string Category { get; set; }

            /// <summary>
            /// The natural log of the category's probability
            /// </summary>
            [ProtoMember(2)]
            public double LogProbability { get; set; }

	        /// <summary>
	        /// The category's probability
	        /// </summary>
	        [ProtoMember(3)]
	        public double Probability { get; set; }
        }

        /// <summary>
        /// A classification and its associated data
        /// </summary>
        [ProtoContract]
        public class ClassSummary
        {
            /// <summary>
            /// The classification label
            /// </summary>
            [ProtoMember(1)]
            public string Label { get; set; }

            /// <summary>
            /// The natural log of the prior
            /// </summary>
            [ProtoMember(2)]
            public double LogPrior { get; set; }

            /// <summary>
            /// The column data associated with this classification
            /// </summary>
            [ProtoMember(3)]
            public List<Column> ColumnSummary { get; set; }

	        /// <summary>
	        /// The classification prior probability
	        /// </summary>
	        [ProtoMember(4)]
	        public double Prior { get; set; }
        }

        /// <summary>
        /// A list of possible classifications and their data
        /// </summary>
        [ProtoMember(1)]
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
