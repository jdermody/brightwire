using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Icbld.BrightWire.Models
{
    [ProtoContract]
    public class NetworkLayer
    {
        [ProtoMember(1)]
        public int InputSize { get; set; }

        [ProtoMember(2)]
        public int OutputSize { get; set; }

        [ProtoMember(3)]
        public ActivationType Activation { get; set; }

        [ProtoMember(4)]
        public WeightInitialisationType WeightInitialisation { get; set; }

        [ProtoMember(5)]
        public RegularisationType Regularisation { get; set; }

        [ProtoMember(6)]
        public WeightUpdateType WeightUpdate { get; set; }

        [ProtoMember(7)]
        public LayerTrainerType LayerTrainer { get; set; }

        [ProtoMember(8)]
        public float Lambda { get; set; }

        [ProtoMember(9)]
        public float Momentum { get; set; }

        [ProtoMember(10)]
        public float DecayRate { get; set; }

        [ProtoMember(11)]
        public float DecayRate2 { get; set; }

        [ProtoMember(12)]
        public float Dropout { get; set; }

        [ProtoMember(13)]
        public FloatArray Bias { get; set; }

        [ProtoMember(14)]
        public FloatArray[] Weight { get; set; }

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
