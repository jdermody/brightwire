using BrightWire.TabularData;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightWire.Helper
{
	/// <summary>
	/// Simple CSV parser that attempts to detect data column types
	/// </summary>
	internal class CSVParser
	{
		static readonly HashSet<string> _booleanTrueFields = new HashSet<string>();
		static readonly HashSet<string> _booleanFalseFields = new HashSet<string>();
		static CSVParser()
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
		readonly bool _parseAsText;

		public CSVParser(char delimiter = ',', bool parseAsText = false)
		{
			_delimiter = delimiter;
			_parseAsText = parseAsText;
		}

		string _ReadLine(StreamReader reader)
		{
			var ret = new StringBuilder();
			var inQuote = false;

			while (!reader.EndOfStream) {
				var ch = (char)reader.Read();
				if (ch == '"')
					inQuote = !inQuote;
				else if ((ch == '\r' || ch == '\n') && !inQuote) {
					if(ret.Length > 0)
						return ret.ToString();
					continue;
				}
				else if (inQuote && ch == _delimiter && Char.IsWhiteSpace(_delimiter))
					inQuote = false;
				ret.Append(ch);
			}
			return ret.ToString();
		}

		public IDataTable Parse(StreamReader reader, Stream output = null, bool? hasHeader = null, int previewLines = 1024)
		{
			if (output == null)
				output = new MemoryStream();

			// load the first batch of lines to work out the column types
			var lines = new List<string>();
			while (!reader.EndOfStream && lines.Count < previewLines) {
				var line = _ReadLine(reader);
				if (String.IsNullOrEmpty(line))
					continue;
				lines.Add(line);
			}

			if (lines.Any()) {
				// use the preview to determine the data type
				bool hasHeaderRow = hasHeader ?? false;
				var writer = _DetermineHeaders(output, lines, !hasHeader.HasValue, ref hasHeaderRow);

				// add the preview lines
				foreach (var line in lines.Skip(hasHeaderRow ? 1 : 0))
					_Preview(line, writer);

				// parse the remaining data
				while (!reader.EndOfStream) {
					var line = _ReadLine(reader);
					if (String.IsNullOrEmpty(line))
						continue;
					_Preview(line, writer);
				}

				// reset the stream
				reader.BaseStream.Seek(0, SeekOrigin.Begin);

				// parse the entire stream with the final set of column types
				var hasFoundHeader = false;
				while (!reader.EndOfStream) {
					var line = _ReadLine(reader);
					if (String.IsNullOrEmpty(line))
						continue;
					if (hasHeaderRow && !hasFoundHeader)
						hasFoundHeader = true;
					else
						_Add(line, writer);
				}
				return writer.GetDataTable();
			}
			return null;
		}

		DataTableWriter _DetermineHeaders(Stream stream, List<string> lines, bool checkForHeader, ref bool hasHeader)
		{
			// see if there is a header (all strings)
			var firstLineTypes = _Parse(lines.First()).ToList();
			if (checkForHeader)
				hasHeader = firstLineTypes.All(str => _DetermineType(str) == ColumnType.String);

			// get the list of header names
			var headerNames = new List<string>();
			int index = 0;
			foreach (var item in firstLineTypes)
				headerNames.Add(hasHeader ? item : "_col" + index++);

			// get the list of column types
			IReadOnlyList<ColumnType> columnTypes;
			if (_parseAsText)
				columnTypes = firstLineTypes.Select(c => ColumnType.String).ToList();
			else {
				columnTypes = lines
					.Skip(hasHeader ? 1 : 0)
					.SelectMany(line => _Parse(line).Select((str, pos) => Tuple.Create(str, pos)))
					.GroupBy(l => l.Item2, l => _DetermineType(l.Item1))
					.OrderBy(g => g.Key)
					.Select(_GetColumnType)
					.ToList()
				;
			}

			// add the columns
			var ret = new DataTableWriter(stream);
			foreach (var column in headerNames.Zip(columnTypes, Tuple.Create))
				ret.AddColumn(column.Item1, column.Item2);

			return ret;
		}

		ColumnType _GetColumnType(IGrouping<int, ColumnType> group)
		{
			var ret = (ColumnType)group.Max(v => (int)v);
			if (ret == ColumnType.Date) {
				if (group.All(d => d == ColumnType.Date))
					return ret;
				return ColumnType.String;
			}
			return ret;
		}

		ColumnType _DetermineType(string str)
		{
			if (sbyte.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
				return ColumnType.Byte;
			else if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
				return ColumnType.Int;
			else if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
				return ColumnType.Long;
			else if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
				return ColumnType.Float;
			else if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
				return ColumnType.Double;
			else if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var _))
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
						} else
							inQuote = false;
					} else
						inQuote = true;
				} else if (ch == _delimiter && !inQuote) {
					yield return curr.ToString();
					curr.Clear();
				} else
					curr.Append(ch);
			}
			if (curr.Length > 0)
				yield return curr.ToString();
		}

		private void _Preview(string line, DataTableWriter writer)
		{
			int index = 0;
			var fields = _Parse(line).ToList();

			// append empty strings to fill any blanks
			for (int i = fields.Count, len = writer.Columns.Count; i < len; i++)
				fields.Add(String.Empty);

			foreach (var column in writer.Columns)
				_Convert(ref column._type, fields[index++]);
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
			if (_parseAsText)
				return str;

			int i;
			long l;
			//DateTime dt;

			switch (type) {
				case ColumnType.Boolean:
					return _booleanTrueFields.Contains(str.ToUpperInvariant());

				case ColumnType.Date:
					return DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

				case ColumnType.Double:
					return double.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

				case ColumnType.Float:
					if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out float flt))
						return flt;
					if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double dbl)) {
						type = ColumnType.Double;
						return dbl;
					}
					type = ColumnType.String;
					return str;

				case ColumnType.Long:
					return long.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

				case ColumnType.Int:
					if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
						return i;
					if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out l)) {
						type = ColumnType.Long;
						return l;
					}
					type = ColumnType.String;
					return str;

				case ColumnType.Byte:
					if (sbyte.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out sbyte b))
						return b;
					if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out i)) {
						type = ColumnType.Int;
						return i;
					}
					if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out l)) {
						type = ColumnType.Long;
						return l;
					}
					type = ColumnType.String;
					return str;

				default:
					return str;
			}
		}
	}
}
