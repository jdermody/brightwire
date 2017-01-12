using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BrightWire.Models
{
    [ProtoContract]
    public class ConvolutionalNetwork
    {
        public enum ConvolutionalLayerType
        {
            Convolutional,
            MaxPooling
        }

        [ProtoContract]
        public class Layer
        {
            [ProtoMember(1)]
            public ConvolutionalLayerType Type { get; set; }

            [ProtoMember(2)]
            public NetworkLayer Data { get; set; }

            [ProtoMember(3)]
            public int FilterWidth { get; set; }

            [ProtoMember(4)]
            public int FilterHeight { get; set; }

            [ProtoMember(5)]
            public int Stride { get; set; }

            [ProtoMember(6)]
            public int Padding { get; set; }

            public void WriteTo(XmlWriter writer)
            {
                writer.WriteStartElement("convolutional-layer");
                writer.WriteAttributeString("type", Type.ToString());
                writer.WriteAttributeString("filter-width", FilterWidth.ToString());
                writer.WriteAttributeString("filter-height", FilterHeight.ToString());
                writer.WriteAttributeString("stride", Stride.ToString());
                if(Padding > 0)
                    writer.WriteAttributeString("pading", Padding.ToString());

                if (Data != null)
                    Data.WriteTo(writer);
                writer.WriteEndElement();
            }
        }

        [ProtoMember(1)]
        public Layer[] ConvolutionalLayer { get; set; }

        [ProtoMember(2)]
        public FeedForwardNetwork FeedForward { get; set; }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("convolutional-network");
            if (ConvolutionalLayer != null) {
                foreach (var item in ConvolutionalLayer)
                    item.WriteTo(writer);
            }
            if (FeedForward != null)
                FeedForward.WriteTo(writer);
            writer.WriteEndElement();
        }
    }
}
