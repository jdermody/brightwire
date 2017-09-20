using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;

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
        public FloatVector[] Row { get; set; }

        /// <summary>
        /// Create a new float matrix with the specified rows
        /// </summary>
        /// <param name="rows">The rows in the matrix (each vector should be the same length)</param>
        public static FloatMatrix Create(FloatVector[] rows)
        {
#if DEBUG
            if (rows != null && rows.All(r => r != null)) {
                var firstRowSize = rows.First().Size;
                Debug.Assert(rows.All(r => r.Size == firstRowSize));
            }
#endif
            return new FloatMatrix {
                Row = rows
            };
        }

        /// <summary>
        /// Create a new float matrix with the specified number of rows and columns initialised to zero
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        public static FloatMatrix Create(int rowCount, int columnCount)
        {
            return new FloatMatrix {
                Row = Enumerable.Range(0, rowCount)
                    .Select(i => FloatVector.Create(columnCount))
                    .ToArray()
            };
        }

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
            var ret = new FloatVector[len];
            for (var i = 0; i < len; i++)
                ret[i] = FloatVector.ReadFrom(reader);

            return Create(ret);
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

        /// <summary>
        /// Tests if the matrices are the same
        /// </summary>
        /// <param name="matrix">The matrix to compare</param>
        /// <param name="comparer">Optional IEqualityComparer to use</param>
        /// <returns></returns>
        public bool IsEqualTo(FloatMatrix matrix, IEqualityComparer<float> comparer = null)
        {
            if (matrix == null || RowCount != matrix.RowCount || ColumnCount != matrix.ColumnCount)
                return false;

            for(int i = 0, len = RowCount; i < len; i++) {
                if (!Row[i].IsEqualTo(matrix.Row[i], comparer))
                    return false;
            }
            return true;
        }
    }
}
