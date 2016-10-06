using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class NaiveBayes
    {
        public enum ColumnType
        {
            ContinuousGaussian,
            Categorical
        }
        public interface IColumn
        {
            ColumnType Type { get; }
            int ColumnIndex { get; }
        }

        [ProtoContract]
        public class ContinuousGaussianColumn : IColumn
        {
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

            [ProtoMember(2)]
            public double Variance { get; set; }

            [ProtoMember(3)]
            public double Mean { get; set; }

            public ColumnType Type { get { return ColumnType.ContinuousGaussian; } }
        }

        [ProtoContract]
        public class CategorialProbability
        {
            [ProtoMember(1)]
            public string Category { get; set; }

            [ProtoMember(2)]
            public double LogProbability { get; set; }
        }

        [ProtoContract]
        public class CategorialColumn : IColumn
        {
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

            [ProtoMember(2)]
            public List<CategorialProbability> Probability { get; set; }

            public ColumnType Type { get { return ColumnType.Categorical; } }
        }

        [ProtoContract]
        public class ClassSummary
        {
            [ProtoMember(1)]
            public string Label { get; set; }

            [ProtoMember(2)]
            public List<IColumn> ColumnSummary { get; set; }
        }

        [ProtoMember(1)]
        public List<ClassSummary> Class { get; set; }


    }
}
