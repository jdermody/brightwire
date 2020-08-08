using BrightWire.TreeBased;

namespace BrightWire.Models
{
    /// <summary>
    /// A random forest model
    /// </summary>
    public class RandomForest
    {
        /// <summary>
        /// The list of trees in the forest
        /// </summary>
        public DecisionTree[] Forest { get; set; }

        /// <summary>
        /// Creates a classifier from the model
        /// </summary>
        /// <returns></returns>
        public IRowClassifier CreateClassifier()
        {
            return new RandomForestClassifier(this);
        }
    }
}
