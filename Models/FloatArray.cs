using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// An array of floats
    /// </summary>
    [ProtoContract]
    public class FloatArray
    {
        /// <summary>
        /// The data
        /// </summary>
        [ProtoMember(1)]
        public float[] Data { get; set; }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return String.Format("Size: {0}", Data.Length);
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name);
            if(Data != null)
                writer.WriteValue(String.Join("|", Data));
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data.Length);
            foreach (var val in Data)
                writer.Write(val);
        }

        /// <summary>
        /// Creates a float array from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static FloatArray ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new float[len];
            for (var i = 0; i < len; i++)
                ret[i] = reader.ReadSingle();
            return new FloatArray {
                Data = ret
            };
        }
    }
}
