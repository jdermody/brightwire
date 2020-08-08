using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ProtoBuf;

namespace BrightWire.Models
{
	/// <summary>
	/// 
	/// </summary>
	[ProtoContract]
	public class ConfusionMatrix
	{
		[ProtoContract]
		public class ActualClassification
		{
			[ProtoMember(1)]
			public int ClassificationIndex { get; set; }

			[ProtoMember(2)]
			public uint Count { get; set; }
		}

		[ProtoContract]
		public class ExpectedClassification
		{
			[ProtoMember(1)]
			public int ClassificationIndex { get; set; }

			[ProtoMember(2)]
			public ActualClassification[] ActualClassifications { get; set; }
		}

		[ProtoMember(1)]
		public string[] ClassificationLabels { get; set; }

		[ProtoMember(2)]
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
