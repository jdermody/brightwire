using System.IO;
using BrightData;
using BrightWire.Helper;
using BrightWire.TreeBased;

namespace BrightWire.Models.TreeBased
{
    /// <summary>
    /// A random forest model
    /// </summary>
    public class RandomForest : ISerializable
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

        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
