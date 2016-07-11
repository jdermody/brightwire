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
    [ProtoContract]
    public class FloatArray
    {
        [ProtoMember(1)]
        public float[] Data { get; set; }

        public override string ToString()
        {
            return String.Format("Size: {0}", Data.Length);
        }

        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name);
            if(Data != null)
                writer.WriteValue(String.Join("|", Data));
            writer.WriteEndElement();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data.Length);
            foreach (var val in Data)
                writer.Write(val);
        }

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
