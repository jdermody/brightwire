using BrightWire.TabularData;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class ClassificationSet
    {
        [ProtoContract]
        public class Classification
        {
            [ProtoMember(1)]
            public string Name { get; set; }

            [ProtoMember(2)]
            public uint[] Data { get; set; }
        }

        [ProtoMember(1)]
        public Classification[] Classifications { get; set; }

        public IReadOnlyList<Tuple<IVector, string>> Encode(ILinearAlgebraProvider lap, bool normalise = true)
        {
            int maxCount = 0;
            var max = Classifications.SelectMany(d => d.Data).Max();
            var data = new List<Tuple<float[], string>>();

            foreach(var item in Classifications) {
                var vector = new float[max+1];
                foreach (var index in item.Data.GroupBy(d => d).Select(g => Tuple.Create(g.Key, g.Count()))) {
                    var count = index.Item2;
                    if (count > maxCount)
                        maxCount = count;
                    vector[index.Item1] = count;
                }
                data.Add(Tuple.Create(vector, item.Name));
            }
            if(normalise) {
                float denominator = maxCount;
                foreach(var item in data) {
                    for (var i = 0; i <= max; i++)
                        item.Item1[i] /= denominator;
                }
            }
            return data.Select(d => Tuple.Create(lap.Create(d.Item1), d.Item2)).ToList();
        }

        public IDataTable Encode(Stream stream = null)
        {
            var max = Classifications.SelectMany(d => d.Data).Max();
            var dataTable = new MutableDataTable();
            for(var i = 0; i < max; i++)
                dataTable.AddColumn(ColumnType.Int, "term " + i.ToString()).IsContinuous = true;
            dataTable.AddColumn(ColumnType.String, "classification", true);

            foreach (var item in Classifications) {
                var data = new object[max+1];
                for (var i = 0; i < max; i++)
                    data[i] = (int)0;
                foreach (var index in item.Data.GroupBy(d => d).Select(g => Tuple.Create(g.Key, g.Count())))
                    data[index.Item1] = index.Item2;
                data[max] = item.Name;
                dataTable.AddRow(data);
            }

            return dataTable.Index(stream);
        }
    }
}
