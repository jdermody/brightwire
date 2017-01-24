using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A convolutional neural network
    /// </summary>
    [ProtoContract]
    public class ConvolutionalNetwork
    {
        /// <summary>
        /// Convolutional layer type
        /// </summary>
        public enum ConvolutionalLayerType
        {
            /// <summary>
            /// Convolution operation
            /// </summary>
            Convolutional,

            /// <summary>
            /// Max pooling operation
            /// </summary>
            MaxPooling
        }

        /// <summary>
        /// A convolutional layer
        /// </summary>
        [ProtoContract]
        public class Layer
        {
            /// <summary>
            /// The layer type
            /// </summary>
            [ProtoMember(1)]
            public ConvolutionalLayerType Type { get; set; }

            /// <summary>
            /// The layer data
            /// </summary>
            [ProtoMember(2)]
            public NetworkLayer Data { get; set; }

            /// <summary>
            /// The convolution filter width
            /// </summary>
            [ProtoMember(3)]
            public int FilterWidth { get; set; }

            /// <summary>
            /// The convolution filter height
            /// </summary>
            [ProtoMember(4)]
            public int FilterHeight { get; set; }

            /// <summary>
            /// The convolution filter stride
            /// </summary>
            [ProtoMember(5)]
            public int Stride { get; set; }

            /// <summary>
            /// Padding to apply to the input before the convolution
            /// </summary>
            [ProtoMember(6)]
            public int Padding { get; set; }

            /// <summary>
            /// Writes the layer as XML
            /// </summary>
            /// <param name="writer">The XML writer</param>
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

        /// <summary>
        /// The list of convolutional layers
        /// </summary>
        [ProtoMember(1)]
        public Layer[] ConvolutionalLayer { get; set; }

        /// <summary>
        /// The list of feed forward layers
        /// </summary>
        [ProtoMember(2)]
        public FeedForwardNetwork FeedForward { get; set; }

        /// <summary>
        /// Writes the network as XML
        /// </summary>
        /// <param name="writer">The XML writer</param>
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
