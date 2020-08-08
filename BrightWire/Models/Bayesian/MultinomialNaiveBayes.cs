using BrightWire.Bayesian;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// Multinomial naive bayes model
    /// </summary>
    public class MultinomialNaiveBayes
    {
        /// <summary>
        /// The conditional probability associated with a string index
        /// </summary>
        public class StringIndexProbability
        {
            /// <summary>
            /// The string index
            /// </summary>
            public uint StringIndex { get; set; }

            /// <summary>
            /// The conditional probability
            /// </summary>
            public double ConditionalProbability { get; set; }
        }

        /// <summary>
        /// Classification data
        /// </summary>
        public class Class
        {
            /// <summary>
            /// The classification label
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// The classification's prior log probability
            /// </summary>
            public double Prior { get; set; }

            /// <summary>
            /// The classifications missing log probability
            /// </summary>
            public double MissingProbability { get; set; }

            /// <summary>
            /// The list of string indexes and their probability
            /// </summary>
            public StringIndexProbability[] Index { get; set; }
        }

        /// <summary>
        /// The list of possible classifications
        /// </summary>
        public Class[] ClassData { get; set; }

        /// <summary>
        /// Creates a classifier from the model
        /// </summary>
        /// <returns></returns>
        public IIndexListClassifier CreateClassifier()
        {
            return new MultinomialNaiveBayesClassifier(this);
        }
    }
}
