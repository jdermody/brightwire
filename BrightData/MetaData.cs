﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightData
{
    /// <inheritdoc />
    public class MetaData : IMetaData
    {
        readonly Dictionary<string, IConvertible> _values = new();
        readonly List<string> _orderedValues = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metaData">Existing meta data to copy from</param>
        /// <param name="keys">Keys to copy (or all if none specified)</param>
        public MetaData(IMetaData? metaData = null, params string[] keys)
        {
            if (metaData != null) {
                var md = (MetaData)metaData;
                foreach (var item in keys) {
                    if(md._values.TryGetValue(item, out var val))
                        _values.Add(item, val);
                }

                var keySet = new HashSet<string>(keys);
                _orderedValues.AddRange(md._orderedValues.Where(v => keySet.Contains(v)));
            }
        }

        /// <inheritdoc />
        public MetaData(IHaveMetaData metaData, params string[] keys) : this(metaData.MetaData, keys) { }

        /// <summary>
        /// Creates meta data from a binary reader
        /// </summary>
        /// <param name="reader">Reader</param>
        public MetaData(BinaryReader reader)
        {
            ReadFrom(reader);
        }

        /// <inheritdoc />
        public void CopyTo(IMetaData metadata)
        {
            var other = (MetaData)metadata;
            var keys = _orderedValues.ToList();

            foreach (var key in keys) {
                if (other._values.ContainsKey(key))
                    other._values[key] = _values[key];
                else {
                    other._orderedValues.Add(key);
                    other._values.Add(key, _values[key]);
                }
            }
        }

        /// <inheritdoc />
        public void CopyTo(IMetaData metadata, params string[] keysToCopy)
        {
            var other = (MetaData) metadata;
            var keySet = new HashSet<string>(keysToCopy);
            var keys = _orderedValues.AsEnumerable().Where(k => keySet.Contains(k));

            foreach (var key in keys) {
                if (other._values.ContainsKey(key))
                    other._values[key] = _values[key];
                else {
                    other._orderedValues.Add(key);
                    other._values.Add(key, _values[key]);
                }
            }
        }

        /// <inheritdoc />
        public void CopyAllExcept(IMetaData metadata, params string[] keys)
        {
            var except = new HashSet<string>(keys);
            CopyTo(metadata, _orderedValues.Where(v => !except.Contains(v)).ToArray());
        }

        /// <inheritdoc />
        public object? Get(string name)
        {
            if (_values.TryGetValue(name, out var obj))
                return obj;
            return null;
        }

        /// <inheritdoc />
        public T? GetNullable<T>(string name) where T : struct
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            return null;
        }

        /// <inheritdoc />
        public T Get<T>(string name, T valueIfMissing) 
            where T : IConvertible
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            return valueIfMissing;
        }

        /// <inheritdoc />
        public T Get<T>(string name) where T : IConvertible
        {
            if (_values.TryGetValue(name, out var obj))
                return (T)obj;
            throw new Exception("Named value not found");
        }

        /// <inheritdoc />
        public T Set<T>(string name, T value) where T : IConvertible
        {
            Set(name, (IConvertible) value);
            return value;
        }

        /// <inheritdoc />
        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                using (var writer = XmlWriter.Create(new StringWriter(ret), new XmlWriterSettings {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8
                })) {
                    writer.WriteStartElement("metadata");
                    foreach (var item in GetNonEmpty()) {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("name", item.Name);
                        writer.WriteAttributeString("type", item.Value.GetTypeCode().ToType()?.ToString() ?? "???");
                        writer.WriteValue(item.String);
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

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            var items = GetNonEmpty().ToList();
            writer.Write(items.Count);
            foreach(var item in items) {
                writer.Write(item.Name);
                var typeCode = item.Value.GetTypeCode();
                writer.Write((byte)typeCode);
                writer.Write(item.String);
            }
        }

        /// <inheritdoc />
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
                    if(typeConverter.ConvertFromString(str) is IConvertible obj)
                        Set(name, obj);
                }
            }
        }

        /// <inheritdoc />
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
            return typeCode switch {
                TypeCode.Double   => ((double) value).ToString("G17"),
                TypeCode.Single   => ((float) value).ToString("G9"),
                TypeCode.DateTime => ((DateTime) value).ToString("o"),
                _                 => value.ToString(CultureInfo.InvariantCulture)
            };
        }

        IEnumerable<(string Name, IConvertible Value, string String)> GetNonEmpty()
        {
            var nonNull = _values.ToDictionary(d => d.Key, d => d.Value);
            foreach (var item in _orderedValues) {
                if (nonNull.TryGetValue(item, out var val)) {
                    var str = Write(val);
                    if(!string.IsNullOrWhiteSpace(str))
                        yield return (item, val, str);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(", ", _orderedValues.Select(v => v + ": " + _values[v].ToString(CultureInfo.InvariantCulture)));
        }

        /// <inheritdoc />
        public bool Has(string key) => _values.ContainsKey(key);

        /// <inheritdoc />
        public void Remove(string key)
        {
            if(_values.Remove(key))
                _orderedValues.Remove(key);
        }

        /// <inheritdoc />
        public IEnumerable<string> AllKeys => _orderedValues;

        /// <inheritdoc />
        public IMetaData Clone()
        {
            var ret = new MetaData();
            CopyTo(ret);
            return ret;
        }
    }
}
