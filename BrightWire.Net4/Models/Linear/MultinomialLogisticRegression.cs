using BrightWire.Linear;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    /// <summary>
    /// Multinomial logistic regression model
    /// </summary>
    [ProtoContract]
    public class MultinomialLogisticRegression
    {
        /// <summary>
        /// The list of logistic regression models
        /// </summary>
        [ProtoMember(1)]
        public LogisticRegression[] Model { get; set; }

        /// <summary>
        /// The associated classification labels
        /// </summary>
        [ProtoMember(2)]
        public string[] Classification { get; set; }

        /// <summary>
        /// The columns used to build the dense input vectors
        /// </summary>
        [ProtoMember(3)]
        public int[] FeatureColumn { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        public IRowClassifier CreateClassifier(ILinearAlgebraProvider lap)
        {
            return new MultinomialLogisticRegressionClassifier(lap, this);
        }
    }
}
