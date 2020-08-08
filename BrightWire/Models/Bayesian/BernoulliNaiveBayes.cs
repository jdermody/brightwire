using BrightWire.Bayesian;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A bernoulli naive bayes model
    /// </summary>
    public class BernoulliNaiveBayes
    {
        /// <summary>
        /// The probabilities associated with a string index
        /// </summary>
        public class StringIndexProbability
        {
            /// <summary>
            /// The string index
            /// </summary>
            public uint StringIndex { get; set; }

            /// <summary>
            /// The log of the conditional probability
            /// </summary>
            public double ConditionalProbability { get; set; }

            /// <summary>
            /// The log of the inverse conditional probability
            /// </summary>
            public double InverseProbability { get; set; }
        }

        /// <summary>
        /// A classification
        /// </summary>
        public class Class
        {
            /// <summary>
            /// The classification label
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// The log of the prior probablilty for this classification
            /// </summary>
            public double Prior { get; set; }

            /// <summary>
            /// The log of the missing probability
            /// </summary>
            public double MissingProbability { get; set; }

            /// <summary>
            /// The list of probabilities for each string index
            /// </summary>
            public StringIndexProbability[] Index { get; set; }

            /// <summary>
            /// The log of the inverse missing probability
            /// </summary>
            public double InverseMissingProbability { get; set; }
        }

        /// <summary>
        /// Classification data
        /// </summary>
        public Class[] ClassData { get; set; }

        /// <summary>
        /// The list of string indexes that were in the training set
        /// </summary>
        public uint[] Vocabulary { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <returns></returns>
        public IIndexListClassifier CreateClassifier()
        {
            return new BernoulliNaiveBayesClassifier(this);
        }
    }
}
