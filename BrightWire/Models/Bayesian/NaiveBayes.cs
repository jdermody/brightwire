using BrightWire.Bayesian;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.Helper;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A naive bayes model
    /// </summary>
    public class NaiveBayes : ISerializable
    {
        /// <summary>
        /// The type of data within the column
        /// </summary>
        public enum ColumnType : byte
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
        public class Column : ISerializable
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
	        public CategorialProbability[] Probability { get; set; }

            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// A category and its associated log probability
        /// </summary>
        public class CategorialProbability : ISerializable
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

            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// A classification and its associated data
        /// </summary>
        public class ClassSummary : ISerializable
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
            public Column[] ColumnSummary { get; set; }

	        /// <summary>
	        /// The classification prior probability
	        /// </summary>
	        public double Prior { get; set; }

            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// A list of possible classifications and their data
        /// </summary>
        public ClassSummary[] Class { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <returns></returns>
        public IRowClassifier CreateClassifier()
        {
            return new NaiveBayesClassifier(this);
        }

        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
