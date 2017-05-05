using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A protobuf serialised matrix
    /// </summary>
    [ProtoContract]
    public class FloatMatrix
    {
        /// <summary>
        /// The rows of the matrix
        /// </summary>
        [ProtoMember(1)]
        public FloatArray[] Row { get; set; }

        /// <summary>
        /// The number of rows
        /// </summary>
        public int RowCount { get { return Row?.Length ?? 0; } }

        /// <summary>
        /// The number of columns
        /// </summary>
        public int ColumnCount { get { return Row?.FirstOrDefault()?.Size ?? 0; } }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return String.Format($"Rows: {RowCount}, Columns: {ColumnCount}");
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "matrix");
            if (Row != null) {
                foreach (var row in Row)
                    row.WriteTo("row", writer);
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(RowCount);
            if (Row != null) {
                foreach (var row in Row)
                    row.WriteTo(writer);
            }
        }

        /// <summary>
        /// Creates a float matrix from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static FloatMatrix ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new FloatArray[len];
            for (var i = 0; i < len; i++)
                ret[i] = FloatArray.ReadFrom(reader);

            return new FloatMatrix {
                Row = ret
            };
        }

        /// <summary>
        /// Converts the matrix to XML
        /// </summary>
        public string Xml
        {
            get
            {
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(sb, settings))
                    WriteTo(null, writer);
                return sb.ToString();
            }
        }
    }
}
