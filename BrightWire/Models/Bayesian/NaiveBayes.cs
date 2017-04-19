using BrightWire.Bayesian;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// A column of the data table
        /// </summary>
        public interface IColumn
        {
            /// <summary>
            /// The type of the column
            /// </summary>
            ColumnType Type { get; }

            /// <summary>
            /// The column index
            /// </summary>
            int ColumnIndex { get; }
        }

        /// <summary>
        /// A continous column model
        /// </summary>
        [ProtoContract]
        public class ContinuousGaussianColumn : IColumn
        {
            /// <summary>
            /// The column index
            /// </summary>
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

            /// <summary>
            /// The variance of the column values
            /// </summary>
            [ProtoMember(2)]
            public double Variance { get; set; }

            /// <summary>
            /// The mean of the column values
            /// </summary>
            [ProtoMember(3)]
            public double Mean { get; set; }

            /// <summary>
            /// The column type
            /// </summary>
            public ColumnType Type { get { return ColumnType.ContinuousGaussian; } }
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
            /// The log of the category's probability
            /// </summary>
            [ProtoMember(2)]
            public double LogProbability { get; set; }
        }

        /// <summary>
        /// A categorical column model
        /// </summary>
        [ProtoContract]
        public class CategorialColumn : IColumn
        {
            /// <summary>
            /// The column index
            /// </summary>
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

            /// <summary>
            /// The list of categories within the column and their probability
            /// </summary>
            [ProtoMember(2)]
            public List<CategorialProbability> Probability { get; set; }

            /// <summary>
            /// The column type
            /// </summary>
            public ColumnType Type { get { return ColumnType.Categorical; } }
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
            /// The classification prior probability
            /// </summary>
            [ProtoMember(2)]
            public double Prior { get; set; }

            /// <summary>
            /// The column data associated with this classification
            /// </summary>
            [ProtoMember(3)]
            public List<IColumn> ColumnSummary { get; set; }
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
