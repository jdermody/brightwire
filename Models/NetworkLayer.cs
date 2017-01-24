using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A neural network layer
    /// </summary>
    [ProtoContract]
    public class NetworkLayer
    {
        /// <summary>
        /// The size of the input vectors
        /// </summary>
        [ProtoMember(1)]
        public int InputSize { get; set; }

        /// <summary>
        /// The size of the output vectors
        /// </summary>
        [ProtoMember(2)]
        public int OutputSize { get; set; }

        /// <summary>
        /// The activation function
        /// </summary>
        [ProtoMember(3)]
        public ActivationType Activation { get; set; }

        /// <summary>
        /// The weight initialisation strategy
        /// </summary>
        [ProtoMember(4)]
        public WeightInitialisationType WeightInitialisation { get; set; }

        /// <summary>
        /// The layer regularisation function
        /// </summary>
        [ProtoMember(5)]
        public RegularisationType Regularisation { get; set; }

        /// <summary>
        /// The weight update strategy
        /// </summary>
        [ProtoMember(6)]
        public WeightUpdateType WeightUpdate { get; set; }

        /// <summary>
        /// The layer trainer type (dropout, dropconnect or none)
        /// </summary>
        [ProtoMember(7)]
        public LayerTrainerType LayerTrainer { get; set; }

        /// <summary>
        /// The regularisation hyper-parameter
        /// </summary>
        [ProtoMember(8)]
        public float Lambda { get; set; }

        /// <summary>
        /// Momentum hyper-parameter
        /// </summary>
        [ProtoMember(9)]
        public float Momentum { get; set; }

        /// <summary>
        /// The rate of decay hyper-parameter
        /// </summary>
        [ProtoMember(10)]
        public float DecayRate { get; set; }

        /// <summary>
        /// Second rate of decay hyper-parameter
        /// </summary>
        [ProtoMember(11)]
        public float DecayRate2 { get; set; }

        /// <summary>
        /// Dropout/drop connect percentage
        /// </summary>
        [ProtoMember(12)]
        public float Dropout { get; set; }

        /// <summary>
        /// The bias vector
        /// </summary>
        [ProtoMember(13)]
        public FloatArray Bias { get; set; }

        /// <summary>
        /// The weight matrix
        /// </summary>
        [ProtoMember(14)]
        public FloatArray[] Weight { get; set; }

        /// <summary>
        /// Writes this layer as XML
        /// </summary>
        /// <param name="writer">The XML writer</param>
        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("input-size", InputSize.ToString());
            writer.WriteAttributeString("output-size", OutputSize.ToString());
            writer.WriteAttributeString("activation", Activation.ToString());
            writer.WriteAttributeString("weight-init", WeightInitialisation.ToString());
            writer.WriteAttributeString("regularisation", Regularisation.ToString());
            writer.WriteAttributeString("weight-update", WeightUpdate.ToString());
            writer.WriteAttributeString("trainer", LayerTrainer.ToString());
            writer.WriteAttributeString("lambda", Lambda.ToString());
            writer.WriteAttributeString("momentum", Momentum.ToString());
            writer.WriteAttributeString("decay-rate", DecayRate.ToString());
            writer.WriteAttributeString("decay-rate2", DecayRate2.ToString());
            writer.WriteAttributeString("dropout", Dropout.ToString());
            if (Bias != null)
                Bias.WriteTo("bias", writer);
            if(Weight != null) {
                writer.WriteStartElement("weight");
                foreach (var item in Weight)
                    item.WriteTo("row", writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
