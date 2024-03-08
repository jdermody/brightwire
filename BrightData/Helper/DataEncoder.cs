using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;

namespace BrightData.Helper
{
    /// <summary>
    /// Encodes types from/to bytes
    /// </summary>
    public class DataEncoder : IDataReader
    {
        readonly BrightDataContext _context;
        internal DataEncoder(BrightDataContext context) => _context = context;

        /// <summary>
        /// Generic method to read from a binary reader
        /// </summary>
        /// <typeparam name="T">Type to read</typeparam>
        /// <param name="reader">Source</param>
        public T Read<T>(BinaryReader reader) where T : notnull
        {
            var typeOfT = typeof(T);

            switch (Type.GetTypeCode(typeOfT)) {
                case TypeCode.String:
                    var str = reader.ReadString();
                    return __refvalue(__makeref(str), T);

                case TypeCode.Decimal:
                    var dec = reader.ReadDecimal();
                    return __refvalue(__makeref(dec), T);

                case TypeCode.Double:
                    var dbl = reader.ReadDouble();
                    return __refvalue(__makeref(dbl), T);

                case TypeCode.Single:
                    var flt = reader.ReadSingle();
                    return __refvalue(__makeref(flt), T);

                case TypeCode.Int64:
                    var l = reader.ReadInt64();
                    return __refvalue(__makeref(l), T);

                case TypeCode.Int32:
                    var int32 = reader.ReadInt32();
                    return __refvalue(__makeref(int32), T);

                case TypeCode.Int16:
                    var int16 = reader.ReadInt16();
                    return __refvalue(__makeref(int16), T);

                case TypeCode.SByte:
                    var sb = reader.ReadSByte();
                    return __refvalue(__makeref(sb), T);

                case TypeCode.Boolean:
                    var b = reader.ReadBoolean();
                    return __refvalue(__makeref(b), T);

                case TypeCode.DateTime:
                    var dt = new DateTime(reader.ReadInt64());
                    return __refvalue(__makeref(dt), T);
            }


            if (typeOfT == typeof(IndexList)) {
                var val = _context.CreateIndexList(reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(WeightedIndexList)) {
                var val = _context.CreateWeightedIndexList(reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(ReadOnlyVector<float>)) {
                var val = _context.CreateReadOnlyVector(reader);
                return __refvalue(__makeref(val), T);
            }
            if (typeOfT == typeof(ReadOnlyMatrix<float>)) {
                var val = _context.CreateReadOnlyMatrix(reader);
                return __refvalue(__makeref(val), T);
            }
            if (typeOfT == typeof(ReadOnlyTensor3D<float>)) {
                var val = _context.CreateReadOnlyTensor3D(reader);
                return __refvalue(__makeref(val), T);
            }
            if (typeOfT == typeof(ReadOnlyTensor4D<float>)) {
                var val = _context.CreateReadOnlyTensor4D(reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(BinaryData)) {
                var val = _context.Create<BinaryData>(reader);
                return __refvalue(__makeref(val), T);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Generic method to read an array from a binary reader
        /// </summary>
        /// <typeparam name="T">Type within the array</typeparam>
        /// <param name="reader">Source</param>
        public T[] ReadArray<T>(BinaryReader reader) where T : notnull
        {
            var typeOfT = typeof(T);
            var typeCode = Type.GetTypeCode(typeOfT);
            var len = reader.ReadUInt32();

            if (typeCode == TypeCode.String) {
                var ret = new string[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = reader.ReadString();
                return __refvalue(__makeref(ret), T[]);
            }

            if (typeCode == TypeCode.Decimal) {
                var ret = ReadStructs<decimal>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.Double) {
                var ret = ReadStructs<double>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.Single) {
                var ret = ReadStructs<float>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.Int64) {
                var ret = ReadStructs<long>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.Int32) {
                var ret = ReadStructs<int>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.Int16) {
                var ret = ReadStructs<short>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.SByte) {
                var ret = ReadStructs<sbyte>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.Boolean) {
                // TODO: encode as bits?
                var ret = ReadStructs<bool>(len, reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeCode == TypeCode.DateTime) {
                var ret = new DateTime[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = new DateTime(reader.ReadInt64());
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(IndexList)) {
                var ret = new IndexList[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = _context.CreateIndexList(reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(WeightedIndexList)) {
                var ret = new WeightedIndexList[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = _context.CreateWeightedIndexList(reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(ReadOnlyVector<float>)) {
                var ret = new ReadOnlyVector<float>[len];
                for (uint i = 0; i < len; i++) {
                    ret[i] = _context.CreateReadOnlyVector(reader);
                }
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(ReadOnlyMatrix<float>)) {
                var ret = new ReadOnlyMatrix<float>[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = _context.CreateReadOnlyMatrix(reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(ReadOnlyTensor3D<float>)) {
                var ret = new ReadOnlyTensor3D<float>[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = _context.CreateReadOnlyTensor3D(reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(ReadOnlyTensor4D<float>)) {
                var ret = new ReadOnlyTensor4D<float>[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = _context.CreateReadOnlyTensor4D(reader);
                return __refvalue(__makeref(ret), T[]);
            }
            if (typeOfT == typeof(BinaryData)) {
                var ret = new BinaryData[len];
                for (uint i = 0; i < len; i++)
                    ret[i] = _context.Create<BinaryData>(reader);
                return __refvalue(__makeref(ret), T[]);
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generic method to write to binary writer
        /// </summary>
        /// <typeparam name="T">Type to write</typeparam>
        /// <param name="writer">Destination</param>
        /// <param name="val">Item to write</param>
        public static void Write<T>(BinaryWriter writer, T val)
        {
            var typeOfT = typeof(T);
            var valRef = __makeref(val);

            if (typeOfT == typeof(string))
                writer.Write(__refvalue(valRef, string));
            else if (typeOfT == typeof(decimal))
                writer.Write(__refvalue(valRef, decimal));
            else if (typeOfT == typeof(double))
                writer.Write(__refvalue(valRef, double));
            else if (typeOfT == typeof(float))
                writer.Write(__refvalue(valRef, float));
            else if (typeOfT == typeof(long))
                writer.Write(__refvalue(valRef, long));
            else if (typeOfT == typeof(int))
                writer.Write(__refvalue(valRef, int));
            else if (typeOfT == typeof(short))
                writer.Write(__refvalue(valRef, short));
            else if (typeOfT == typeof(sbyte))
                writer.Write(__refvalue(valRef, sbyte));
            else if (typeOfT == typeof(bool))
                writer.Write(__refvalue(valRef, bool));
            else if (typeOfT == typeof(DateTime))
                writer.Write(__refvalue(valRef, DateTime).Ticks);
            else if (typeOfT == typeof(IndexList))
                __refvalue(valRef, IndexList).WriteTo(writer);
            else if (typeOfT == typeof(WeightedIndexList))
                __refvalue(valRef, WeightedIndexList).WriteTo(writer);
            else if (typeOfT == typeof(ReadOnlyVector<float>))
                __refvalue(valRef, ReadOnlyVector<float>).WriteTo(writer);
            else if (typeOfT == typeof(ReadOnlyMatrix<float>))
                __refvalue(valRef, ReadOnlyMatrix<float>).WriteTo(writer);
            else if (typeOfT == typeof(ReadOnlyTensor3D<float>))
                __refvalue(valRef, ReadOnlyTensor3D<float>).WriteTo(writer);
            else if (typeOfT == typeof(ReadOnlyTensor4D<float>))
                __refvalue(valRef, ReadOnlyTensor4D<float>).WriteTo(writer);
            else if (typeOfT == typeof(BinaryData))
                __refvalue(valRef, BinaryData).WriteTo(writer);
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Generic method to write an array to a binary writer
        /// </summary>
        /// <typeparam name="T">Type to write</typeparam>
        /// <param name="writer">Destination</param>
        /// <param name="values">Array to write</param>
        public static unsafe void Write<T>(BinaryWriter writer, T[] values)
        {
            var typeOfT = typeof(T);
            var typeCode = Type.GetTypeCode(typeOfT);
            var len = (uint)values.Length;
            writer.Write(len);

            if (typeCode == TypeCode.String) {
                var data = __refvalue(__makeref(values), string[]);
                for (uint i = 0; i < len; i++)
                    writer.Write(data[i]);
            } else if (typeCode == TypeCode.Decimal) {
                fixed (decimal* ptr = __refvalue(__makeref(values), decimal[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(decimal)));
                }
            } else if (typeCode == TypeCode.Double) {
                fixed (double* ptr = __refvalue(__makeref(values), double[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(double)));
                }
            } else if (typeCode == TypeCode.Single) {
                fixed (float* ptr = __refvalue(__makeref(values), float[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(float)));
                }
            } else if (typeCode == TypeCode.Int64) {
                fixed (long* ptr = __refvalue(__makeref(values), long[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(long)));
                }
            } else if (typeCode == TypeCode.Int32) {
                fixed (int* ptr = __refvalue(__makeref(values), int[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(int)));
                }
            } else if (typeCode == TypeCode.Int16) {
                fixed (short* ptr = __refvalue(__makeref(values), short[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(short)));
                }
            } else if (typeCode == TypeCode.SByte) {
                fixed (sbyte* ptr = __refvalue(__makeref(values), sbyte[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(sbyte)));
                }
            } else if (typeCode == TypeCode.Boolean) {
                // TODO: encode as bits?
                fixed (bool* ptr = __refvalue(__makeref(values), bool[])) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(bool)));
                }
            } else if (typeCode == TypeCode.DateTime) {
                var ticks = __refvalue(__makeref(values), DateTime[]).Select(d => d.Ticks).ToArray();
                fixed (long* ptr = ticks) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, (int)len * sizeof(long)));
                }
            } else if (typeOfT == typeof(IndexList)) {
                var data = __refvalue(__makeref(values), IndexList[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else if (typeOfT == typeof(WeightedIndexList)) {
                var data = __refvalue(__makeref(values), WeightedIndexList[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else if (typeOfT == typeof(ReadOnlyVector<float>)) {
                var data = __refvalue(__makeref(values), ReadOnlyVector<float>[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else if (typeOfT == typeof(ReadOnlyMatrix<float>)) {
                var data = __refvalue(__makeref(values), ReadOnlyMatrix<float>[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else if (typeOfT == typeof(ReadOnlyTensor3D<float>)) {
                var data = __refvalue(__makeref(values), ReadOnlyTensor3D<float>[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else if (typeOfT == typeof(ReadOnlyTensor4D<float>)) {
                var data = __refvalue(__makeref(values), ReadOnlyTensor4D<float>[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else if (typeOfT == typeof(BinaryData)) {
                var data = __refvalue(__makeref(values), BinaryData[]);
                for (uint i = 0; i < len; i++)
                    data[i].WriteTo(writer);
            } else
                throw new NotImplementedException();
        }

        static TS[] ReadStructs<TS>(uint len, BinaryReader reader)
            where TS : struct
        {
            return reader.BaseStream.ReadArray<TS>(len);
        }
    }
}
