using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightData.Types
{
    /// <summary>
    /// Unstructured meta data store
    /// </summary>
    public class MetaData
    {
        readonly Dictionary<string, IConvertible> _values = [];
        readonly List<string> _orderedValues = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metaData">Existing meta data to copy from</param>
        /// <param name="keys">Keys to copy (or all if none specified)</param>
        public MetaData(MetaData? metaData = null, params string[] keys)
        {
            if (metaData != null)
            {
                var md = metaData;
                foreach (var item in keys)
                {
                    if (md._values.TryGetValue(item, out var val))
                        _values.Add(item, val);
                }

                var keySet = new HashSet<string>(keys);
                _orderedValues.AddRange(md._orderedValues.Where(v => keySet.Contains(v)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="keys"></param>
        public MetaData(IHaveMetaData metaData, params string[] keys) : this(metaData.MetaData, keys) { }

        /// <summary>
        /// Creates metadata from a binary reader
        /// </summary>
        /// <param name="reader">Reader</param>
        public MetaData(BinaryReader reader)
        {
            ReadFrom(reader);
        }

        /// <summary>
        /// Copies this to another metadata store
        /// </summary>
        /// <param name="metadata">Other meta data store</param>
        /// <param name="keys">Keys to copy (optional)</param>
        public void CopyTo(MetaData metadata, params string[] keys)
        {
            var other = metadata;
            if (keys.Length == 0)
                keys = _orderedValues.ToArray();

            foreach (var key in keys)
            {
                if (other._values.ContainsKey(key))
                    other._values[key] = _values[key];
                else if (_values.TryGetValue(key, out var value))
                {
                    other._orderedValues.Add(key);
                    other._values.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Returns a value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <returns></returns>
        public object? Get(string name)
        {
            return _values.GetValueOrDefault(name);
        }

        /// <summary>
        /// Returns a typed nullable value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetNullable<T>(string name)
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            return default;
        }

        /// <summary>
        /// Returns a typed value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <param name="valueIfMissing">Value to return if the value has not been set</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string name, T valueIfMissing)
            where T : IConvertible
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            return valueIfMissing;
        }

        /// <summary>
        /// Returns an existing value (throws if not found)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the value</param>
        /// <returns></returns>
        public T GetOrThrow<T>(string name) where T : IConvertible
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            throw new Exception("Named value not found");
        }

        /// <summary>
        /// Sets a named value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <param name="value">Value</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Set<T>(string name, T value) where T : IConvertible
        {
            Set(name, (IConvertible)value);
            return value;
        }

        /// <summary>
        /// XML representation of the metadata
        /// </summary>
        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                using (var writer = XmlWriter.Create(new StringWriter(ret), new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8
                }))
                {
                    writer.WriteStartElement("metadata");
                    foreach (var item in GetNonEmpty())
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("name", item.Name);
                        writer.WriteAttributeString("type", item.Value.GetTypeCode().ToType().ToString());
                        writer.WriteValue(item.StringValue);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                return ret.ToString();
            }
        }

        void Set(string name, IConvertible value)
        {
            if (!_values.ContainsKey(name))
                _orderedValues.Add(name);
            _values[name] = value;
        }

        /// <summary>
        /// Writes meta data
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            var items = GetNonEmpty().ToList();
            writer.Write(items.Count);
            foreach (var item in items)
            {
                writer.Write(item.Name);
                var typeCode = item.Value.GetTypeCode();
                writer.Write((byte)typeCode);
                writer.Write(item.StringValue);
            }
        }

        /// <summary>
        /// Reads values from a binary reader
        /// </summary>
        /// <param name="reader"></param>
        public void ReadFrom(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var typeCode = (TypeCode)reader.ReadByte();
                var str = reader.ReadString();
                var type = typeCode.ToType();
                var typeConverter = TypeDescriptor.GetConverter(type);
                if (typeConverter.ConvertFromString(str) is IConvertible obj)
                    Set(name, obj);
            }
        }

        /// <summary>
        /// Returns all value names with the specified prefix
        /// </summary>
        /// <param name="prefix">Prefix to query</param>
        /// <returns></returns>
        public IEnumerable<string> GetStringsWithPrefix(string prefix)
        {
            return _values
                .Where(kv => kv.Key.StartsWith(prefix))
                .Select(kv => kv.Key)
            ;
        }

        static string Write(IConvertible value)
        {
            var typeCode = value.GetTypeCode();
            return typeCode switch
            {
                TypeCode.Double => ((double)value).ToString("G17"),
                TypeCode.Single => ((float)value).ToString("G9"),
                TypeCode.DateTime => ((DateTime)value).ToString("o"),
                _ => value.ToString(CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Returns non empty metadata
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string Name, IConvertible Value, string StringValue)> GetNonEmpty()
        {
            var nonNull = _values.ToDictionary(d => d.Key, d => d.Value);
            foreach (var item in _orderedValues)
            {
                if (nonNull.TryGetValue(item, out var val))
                {
                    var str = Write(val);
                    if (!string.IsNullOrWhiteSpace(str))
                        yield return (item, val, str);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(", ", _orderedValues.Select(v => v + ": " + _values[v].ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Checks if a value has been set
        /// </summary>
        /// <param name="key">Name of the value</param>
        /// <returns></returns>
        public bool Has(string key) => _values.ContainsKey(key);

        /// <summary>
        /// Removes a value
        /// </summary>
        /// <param name="key">Name of the value</param>
        public void Remove(string key)
        {
            if (_values.Remove(key))
                _orderedValues.Remove(key);
        }

        /// <summary>
        /// Returns all keys that have been set
        /// </summary>
        public IEnumerable<string> AllKeys => _orderedValues;

        /// <summary>
        /// Creates a clone of the current metadata
        /// </summary>
        public MetaData Clone()
        {
            var ret = new MetaData();
            CopyTo(ret);
            return ret;
        }
    }
}
