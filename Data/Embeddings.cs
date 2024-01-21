using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Data
{
    public static class Embeddings
    {
        static readonly Lazy<Dictionary<string, string>> _embeddings;
        static readonly Dictionary<string, float[]> _table = [];

        static Embeddings()
        {
            _embeddings = new(() => {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Data.embeddings.txt");
                var reader = new StreamReader(stream!, Encoding.UTF8);
                var ret = new Dictionary<string, string>();
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine()!;
                    var pos = line.IndexOf(':');
                    if(pos > 0)
                        ret.Add(line[..pos], line[(pos+1)..]);
                }
                return ret;
            });
        }

        public static ReadOnlySpan<float> Get(string embedding)
        {
            if (_table.TryGetValue(embedding, out var ret))
                return ret;

            if (_embeddings.Value.TryGetValue(embedding, out var str)) {
                _table.Add(embedding, ret = str.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray()); 
                return ret;
            }
            return [];
        }
    }
}
