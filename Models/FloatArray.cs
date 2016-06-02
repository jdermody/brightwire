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
    }
}
