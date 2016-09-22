using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    public class CSVDataTableBuilder
    {
        static HashSet<string> _booleanTrueFields = new HashSet<string>();
        static HashSet<string> _booleanFalseFields = new HashSet<string>();
        static CSVDataTableBuilder()
        {
            _booleanTrueFields.Add("Y");
            _booleanFalseFields.Add("N");

            _booleanTrueFields.Add("YES");
            _booleanFalseFields.Add("NO");

            _booleanTrueFields.Add("TRUE");
            _booleanFalseFields.Add("FALSE");

            _booleanTrueFields.Add("ON");
            _booleanFalseFields.Add("OFF");
        }

        readonly char _delimiter;

        public CSVDataTableBuilder(char delimiter = ',')
        {
            _delimiter = delimiter;
        }

        public IndexedDataTable Parse(StreamReader reader, Stream output = null, bool? hasHeader = null)
        {
            if (output == null)
                output = new MemoryStream();

            //preview the file
            var lines = new List<string>();
            while (!reader.EndOfStream && lines.Count < IndexedDataTable.BLOCK_SIZE) {
                var line = reader.ReadLine();
                if (String.IsNullOrEmpty(line))
                    continue;
                lines.Add(line);
            }

            if (lines.Any()) {
                // use the preview to determine the data type
                bool hasHeaderRow = hasHeader.HasValue ? hasHeader.Value : false;
                var writer = _DetermineHeaders(output, lines, !hasHeader.HasValue, ref hasHeaderRow);

                // add the preview lines
                foreach (var line in lines.Skip(hasHeaderRow ? 1 : 0))
                    _Add(line, writer);

                // parse the remaining data
                int pos = 0;
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;
                    _Add(line, writer);
                    ++pos;
                }
                return writer.GetIndexedTable();
            }
            return null;
        }

        private DataTableWriter _DetermineHeaders(Stream stream, List<string> lines, bool checkForHeader, ref bool hasHeader)
        {
            // see if there is a header (all strings)
            var firstLineTypes = _Parse(lines.First());
            if (checkForHeader)
                hasHeader = firstLineTypes.All(str => _DetermineType(str) == ColumnType.String);

            // get the list of header names
            var headerNames = new List<string>();
            int index = 0;
            foreach (var item in firstLineTypes)
                headerNames.Add(hasHeader ? item : "_col" + index++);

            // get the list of header types
            var data = lines
                .Skip(hasHeader ? 1 : 0)
                .SelectMany(line => _Parse(line).Select((str, pos) => Tuple.Create(str, pos)))
                .GroupBy(l => l.Item2, l => _DetermineType(l.Item1))
                .OrderBy(g => g.Key)
                .Select(g => g.Min(v => (int)v))
                .Cast<ColumnType>()
                .ToList()
            ;

            // add the columns
            var ret = new DataTableWriter(stream);
            foreach (var column in headerNames.Zip(data, (name, type) => Tuple.Create(name, type)))
                ret.AddColumn(column.Item1, column.Item2);

            return ret;
        }

        ColumnType _DetermineType(string str)
        {
            double dbl;
            float flt;
            long lng;
            int it;
            DateTime date;
            byte b;

            if (byte.TryParse(str, out b))
                return ColumnType.Byte;
            else if (int.TryParse(str, out it))
                return ColumnType.Int;
            else if (long.TryParse(str, out lng))
                return ColumnType.Long;
            else if (float.TryParse(str, out flt))
                return ColumnType.Float;
            else if (double.TryParse(str, out dbl))
                return ColumnType.Double;
            else if (DateTime.TryParse(str, out date))
                return ColumnType.Date;
            else {
                var upperStr = str.ToUpperInvariant();
                if (_booleanTrueFields.Contains(upperStr) || _booleanFalseFields.Contains(upperStr))
                    return ColumnType.Boolean;
                return ColumnType.String;
            }
        }

        IEnumerable<string> _Parse(string line)
        {
            bool inQuote = false;
            var curr = new StringBuilder();

            for (int i = 0, len = line.Length; i < len; i++) {
                var ch = line[i];
                if (ch == '"') {
                    if (inQuote) {
                        if (i + 1 < len && line[i + 1] == '"') {
                            ++i;
                            curr.Append(ch);
                        }
                        else {
                            inQuote = false;
                            continue;
                        }
                    }
                    else {
                        inQuote = true;
                        continue;
                    }
                }
                else if (ch == _delimiter && !inQuote) {
                    yield return curr.ToString();
                    curr.Clear();
                }
                else
                    curr.Append(ch);
            }
            if (curr.Length > 0)
                yield return curr.ToString();
        }

        private void _Add(string line, DataTableWriter writer)
        {
            var convertedData = writer.Columns
                .Zip(_Parse(line), (c, str) => _Convert(ref c._type, str))
                .ToList()
            ;
            writer.AddRow(convertedData);
        }

        private object _Convert(ref ColumnType type, string str)
        {
            double dbl;
            float flt;
            int i;
            byte b;
            long l;

            switch (type) {
                case ColumnType.Boolean:
                    return _booleanTrueFields.Contains(str.ToUpperInvariant());

                case ColumnType.Date:
                    return DateTime.Parse(str);

                case ColumnType.Double:
                    return double.Parse(str);

                case ColumnType.Long:
                    return long.Parse(str);

                case ColumnType.Float:
                    if (float.TryParse(str, out flt))
                        return flt;
                    if(double.TryParse(str, out dbl)) {
                        type = ColumnType.Double;
                        return dbl;
                    }
                    return default(float);

                case ColumnType.Int:
                    if (int.TryParse(str, out i))
                        return i;
                    if(long.TryParse(str, out l)) {
                        type = ColumnType.Long;
                        return l;
                    }
                    return default(int);

                case ColumnType.Byte:
                    if (byte.TryParse(str, out b))
                        return b;
                    if (int.TryParse(str, out i)) {
                        type = ColumnType.Int;
                        return i;
                    }
                    if (long.TryParse(str, out l)) {
                        type = ColumnType.Long;
                        return l;
                    }
                    return default(byte);

                default:
                    return str;
            }
        }
    }
}
