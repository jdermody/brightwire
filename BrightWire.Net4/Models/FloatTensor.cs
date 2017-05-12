using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.IO;

namespace BrightWire.Models
{
    /// <summary>
    /// A protobuf serialised tensor
    /// </summary>
    [ProtoContract]
    public class FloatTensor
    {
        /// <summary>
        /// The list of matrices
        /// </summary>
        [ProtoMember(1)]
        public FloatMatrix[] Matrix { get; set; }

        /// <summary>
        /// The number of rows
        /// </summary>
        public int RowCount { get { return Matrix.FirstOrDefault()?.RowCount ?? 0; } }

        /// <summary>
        /// The number of columns
        /// </summary>
        public int ColumnCount { get { return Matrix.FirstOrDefault()?.ColumnCount ?? 0; } }

        /// <summary>
        /// The depth of the tensor
        /// </summary>
        public int Depth { get { return Matrix?.Length ?? 0; } }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            var first = Matrix?.FirstOrDefault();
            return String.Format($"Rows: {RowCount}, Columns: {ColumnCount}, Depth: {Depth}");
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "tensor");
            if (Matrix != null) {
                foreach (var matrix in Matrix)
                    matrix.WriteTo("matrix", writer);
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Depth);
            if (Matrix != null) {
                foreach (var matrix in Matrix)
                    matrix.WriteTo(writer);
            }
        }

        /// <summary>
        /// Creates a float tensor from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static FloatTensor ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new FloatMatrix[len];
            for (var i = 0; i < len; i++)
                ret[i] = FloatMatrix.ReadFrom(reader);

            return new FloatTensor {
                Matrix = ret
            };
        }

        /// <summary>
        /// Converts the tensor to XML
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

        public int Size => Depth * RowCount * ColumnCount;
    }
}
