using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightWire.Models
{
	/// <summary>
	/// 
	/// </summary>
	public class ConfusionMatrix
	{
		public class ActualClassification
		{
			public int ClassificationIndex { get; set; }

			public uint Count { get; set; }
		}

		public class ExpectedClassification
		{
			public int ClassificationIndex { get; set; }

			public ActualClassification[] ActualClassifications { get; set; }
		}

		public string[] ClassificationLabels { get; set; }

		public ExpectedClassification[] Classifications { get; set; }

		public string AsXml
		{
			get
			{
				var ret = new StringBuilder();
				using (var writer = XmlWriter.Create(new StringWriter(ret))) {
					writer.WriteStartElement("confusion-matrix");
					if (Classifications != null) {
						foreach (var expected in Classifications) {
							writer.WriteStartElement("expected-classification");
							writer.WriteAttributeString("label", ClassificationLabels[expected.ClassificationIndex]);

							if (expected.ActualClassifications != null) {
								foreach (var actual in expected.ActualClassifications) {
									writer.WriteStartElement("actual-classification");
									writer.WriteAttributeString("label", ClassificationLabels[actual.ClassificationIndex]);
									writer.WriteAttributeString("count", actual.Count.ToString());
									writer.WriteEndElement();
								}
							}

							writer.WriteEndElement();
						}
					}
					writer.WriteEndElement();
				}
				return ret.ToString();
			}
		}

		Dictionary<string, int> _classificationTable = null;
		Dictionary<string, int> ClassificationTable
		{
			get
			{
				if(_classificationTable == null) {
					lock(this) {
						if(_classificationTable == null) {
							_classificationTable = ClassificationLabels
								.Select((c, i) => (c, i))
								.ToDictionary(d => d.Item1, d => d.Item2)
							;
						}
					}
				}
				return _classificationTable;
			}
		}

		/// <summary>
		/// Returns the count of the expected vs actual classifications
		/// </summary>
		/// <param name="expected">Expected classification label</param>
		/// <param name="actual">Actual classification label</param>
		/// <returns></returns>
		public uint GetCount(string expected, string actual)
		{
			var expectedIndex = ClassificationTable[expected];
			var expectedClassification = Classifications.FirstOrDefault(c => c.ClassificationIndex == expectedIndex);
			if (expectedClassification != null) {
				var actualIndex = ClassificationTable[actual];
				var actualClassification = expectedClassification.ActualClassifications.FirstOrDefault(c => c.ClassificationIndex == actualIndex);
				if (actualClassification != null)
					return actualClassification.Count;
			}

			return 0;
		}
	}
}
