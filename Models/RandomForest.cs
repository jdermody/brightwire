using BrightWire.TreeBased;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    /// <summary>
    /// A random forest model
    /// </summary>
    [ProtoContract]
    public class RandomForest
    {
        /// <summary>
        /// The list of trees in the forest
        /// </summary>
        [ProtoMember(1)]
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
