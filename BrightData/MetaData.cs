using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightData
{
    public class MetaData : IMetaData
    {
        readonly Dictionary<string, IConvertible> _values = new Dictionary<string, IConvertible>();
        readonly List<string> _orderedValues = new List<string>();

        public MetaData(IMetaData metaData = null, params string[] keys)
        {
            if (metaData != null) {
                var md = (MetaData)metaData;
                var keysToAdd = keys ?? md._values.Select(kv => kv.Key).ToArray();
                foreach (var item in keysToAdd) {
                    if(md._values.TryGetValue(item, out var val))
                        _values.Add(item, val);
                }

                var keySet = new HashSet<string>(keysToAdd);
                _orderedValues.AddRange(md._orderedValues.Where(v => keySet.Contains(v)));
            }
        }

        public MetaData(IHaveMetaData metaData, params string[] keys) : this(metaData?.MetaData, keys) { }

        public MetaData(BinaryReader reader)
        {
            ReadFrom(reader);
        }

        public void CopyTo(IMetaData metadata)
        {
            var other = (MetaData)metadata;
            var keys = _orderedValues.ToList();

            foreach (var key in keys) {
                other._orderedValues.Add(key);
                other._values[key] = _values[key];
            }
        }

        public void CopyTo(IMetaData metadata, params string[] keysToCopy)
        {
            var other = (MetaData) metadata;
            var keySet = new HashSet<string>(keysToCopy);
            var keys = _orderedValues.AsEnumerable().Where(k => keySet.Contains(k));

            foreach (var key in keys) {
                other._orderedValues.Add(key);
                other._values.Add(key, _values[key]);
            }
        }

        public void CopyAllExcept(IMetaData metadata, params string[] keys)
        {
            var except = new HashSet<string>(keys);
            CopyTo(metadata, _orderedValues.Where(v => !except.Contains(v)).ToArray());
        }

        public object Get(string name)
        {
            if (_values.TryGetValue(name, out var obj))
                return obj;
            return null;
        }

        public T Get<T>(string name, T valueIfMissing = default) where T : IConvertible
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            return valueIfMissing;
        }

        public T Set<T>(string name, T value) where T : IConvertible
        {
            _Set(name, value);
            return value;
        }

        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                using var writer = XmlWriter.Create(new StringWriter(ret), new XmlWriterSettings {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8
                });
                writer.WriteStartElement("metadata");
                foreach (var item in _GetNonEmpty()) {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("name", item.Name);
                    writer.WriteAttributeString("type", item.Value.GetTypeCode().ToType().ToString());
                    writer.WriteValue(item.String);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                return ret.ToString();
            }
        }

        void _Set(string name, IConvertible value)
        {
            if (!_values.ContainsKey(name))
                _orderedValues.Add(name);
            _values[name] = value;
        }

        public void WriteTo(BinaryWriter writer)
        {
            var items = _GetNonEmpty().ToList();
            writer.Write(items.Count);
            foreach(var item in items) {
                writer.Write(item.Name);
                var typeCode = item.Value.GetTypeCode();
                writer.Write((byte)typeCode);
                writer.Write(item.String);
            }
        }

        public void ReadFrom(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++) {
                var name = reader.ReadString();
                var typeCode = (TypeCode)reader.ReadByte();
                var str = reader.ReadString();
                var type = typeCode.ToType();

                if (type != null) {
                    var typeConverter = TypeDescriptor.GetConverter(type);
                    var obj = (IConvertible)typeConverter.ConvertFromString(str);
                    _Set(name, obj);
                }
            }
        }

        static string _Write(IConvertible value)
        {
            var typeCode = value.GetTypeCode();
            if (typeCode == TypeCode.Double)
                return ((double)value).ToString("G17");
            if (typeCode == TypeCode.Single)
                return ((float)value).ToString("G9");
            if(typeCode == TypeCode.DateTime)
                return ((DateTime)value).ToString("o");
            return value.ToString(CultureInfo.InvariantCulture);
        }

        IEnumerable<(string Name, IConvertible Value, string String)> _GetNonEmpty()
        {
            var nonNull = _values.Where(kv => kv.Value != null).ToDictionary(d => d.Key, d => d.Value);
            foreach (var item in _orderedValues) {
                if (nonNull.TryGetValue(item, out var val)) {
                    var str = _Write(val);
                    if(!String.IsNullOrWhiteSpace(str))
                        yield return (item, val, str);
                }
            }
        }

        public override string ToString()
        {
            return String.Join(", ", _orderedValues.Select(v => v + ": " + _values[v].ToString(CultureInfo.InvariantCulture)));
        }
    }
}
