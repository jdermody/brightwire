using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using BrightData;

namespace BrightWire.Helper
{
	/// <summary>
	/// Helper classes useful when debugging BW
	/// </summary>
    public static class DebugHelpers
    {
		/// <summary>
		/// Zips two tensors and writes the values side by side
		/// </summary>
		/// <param name="t1">First tensor</param>
		/// <param name="t2">Second tensor</param>
		/// <returns>Xml string</returns>
	    public static string WriteComparison(IIndexable3DFloatTensor t1, IIndexable3DFloatTensor t2)
        {
            using var stringWriter = new StringWriter();
            using var writer = new XmlTextWriter(stringWriter);
            Write(t1, t2, writer);
            writer.Flush();
            return stringWriter.ToString();
        }

		/// <summary>
		/// Zips two tensors and writes the values side by side
		/// </summary>
		/// <param name="t1">First tensor</param>
		/// <param name="t2">Second tensor</param>
		/// <returns>Xml string</returns>
	    public static string WriteComparison(IIndexable4DFloatTensor t1, IIndexable4DFloatTensor t2)
        {
            using var stringWriter = new StringWriter();
            using var writer = new XmlTextWriter(stringWriter);
            Write(t1, t2, writer);
            writer.Flush();
            return stringWriter.ToString();
        }

		/// <summary>
		/// Zips two matrices and writes the values side by side
		/// </summary>
		/// <param name="m1">First matrix</param>
		/// <param name="m2">Second matrix</param>
		/// <returns>Xml string</returns>
	    public static string WriteComparison(IIndexableFloatMatrix m1, IIndexableFloatMatrix m2)
        {
            using var stringWriter = new StringWriter();
            using var writer = new XmlTextWriter(stringWriter);
            Write(m1, m2, writer);
            writer.Flush();
            return stringWriter.ToString();
        }

	    static void Write(IIndexableFloatMatrix m1, IIndexableFloatMatrix m2, XmlTextWriter writer)
	    {
		    writer.WriteStartElement("matrix");
		    writer.WriteAttributeString("r1", m1.RowCount.ToString());
		    writer.WriteAttributeString("r2", m2.RowCount.ToString());
		    foreach (var row in m1.Rows.Zip(m2.Rows, (x, y) => (x, y))) {
			    writer.WriteStartElement("row");
			    writer.WriteAttributeString("c1", row.x.Count.ToString());
			    writer.WriteAttributeString("c2", row.y.Count.ToString());
			    foreach (var cell in row.x.Values.Zip(row.y.Values, (i, j) => (i, j))) {
				    writer.WriteStartElement("cell");
				    writer.WriteAttributeString("v1", cell.i.ToString(CultureInfo.InvariantCulture));
				    writer.WriteAttributeString("v2", cell.j.ToString(CultureInfo.InvariantCulture));
				    writer.WriteEndElement();
			    }
			    writer.WriteEndElement();
		    }
		    writer.WriteEndElement();
	    }

	    static void Write(IIndexable3DFloatTensor t1, IIndexable3DFloatTensor t2, XmlTextWriter writer)
	    {
		    writer.WriteStartElement("tensor-3d");
		    writer.WriteAttributeString("d1", t1.Depth.ToString());
		    writer.WriteAttributeString("d2", t2.Depth.ToString());
		    foreach (var matrix in t1.Matrix.Zip(t2.Matrix, (m1, m2) => (m1, m2)))
			    Write(matrix.m1, matrix.m2, writer);
		    writer.WriteEndElement();
	    }

	    static void Write(IIndexable4DFloatTensor t1, IIndexable4DFloatTensor t2, XmlTextWriter writer)
	    {
		    writer.WriteStartElement("tensor-4d");
		    writer.WriteAttributeString("c1", t1.Count.ToString());
		    writer.WriteAttributeString("c2", t2.Count.ToString());
		    foreach (var tensor in t1.Tensors.Zip(t2.Tensors, (m1, m2) => (m1, m2)))
			    Write(tensor.m1, tensor.m2, writer);
		    writer.WriteEndElement();
	    }
    }
}
