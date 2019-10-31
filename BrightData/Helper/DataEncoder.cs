using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightData.Helper
{
    public class DataEncoder : IDataReader
    {
        readonly IBrightDataContext _context;
        public DataEncoder(IBrightDataContext context) => _context = context;

        public T Read<T>(BinaryReader reader)
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
				var val = IndexList.ReadFrom(_context, reader);
				return __refvalue(__makeref(val), T);
			}

			if (typeOfT == typeof(WeightedIndexList)) {
				var val = WeightedIndexList.ReadFrom(_context, reader);
				return __refvalue(__makeref(val), T);
			}

            if (typeOfT == typeof(Vector<float>)) {
                var val = new Vector<float>(_context, reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(Matrix<float>)) {
                var val = new Matrix<float>(_context, reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(Tensor3D<float>)) {
                var val = new Tensor3D<float>(_context, reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(Tensor4D<float>)) {
                var val = new Tensor4D<float>(_context, reader);
                return __refvalue(__makeref(val), T);
            }

            if (typeOfT == typeof(BinaryData)) {
                var val = new BinaryData(reader);
                return __refvalue(__makeref(val), T);
            }

			throw new NotImplementedException();
		}

        public T[] ReadArray<T>(BinaryReader reader)
		{
			var typeOfT = typeof(T);
			var typeCode = Type.GetTypeCode(typeOfT);
			var len = reader.ReadUInt32();
			var ret = new T[len];

			if (typeCode == TypeCode.String) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadString();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

            else if (typeCode == TypeCode.Decimal) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadDecimal();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeCode == TypeCode.Double) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadDouble();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeCode == TypeCode.Single) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadSingle();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

            else if (typeCode == TypeCode.Int64) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadInt64();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeCode == TypeCode.Int32) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadInt32();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

            else if (typeCode == TypeCode.Int16) {
                for (uint i = 0; i < len; i++) {
                    var val = reader.ReadInt16();
                    ret[i] = __refvalue(__makeref(val), T);
                }
            }

			else if (typeCode == TypeCode.SByte) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadSByte();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeCode == TypeCode.Boolean) {
				for (uint i = 0; i < len; i++) {
					var val = reader.ReadBoolean();
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeCode == TypeCode.DateTime) {
				for (uint i = 0; i < len; i++) {
					var val = new DateTime(reader.ReadInt64());
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeOfT == typeof(IndexList)) {
				for (uint i = 0; i < len; i++) {
					var val = IndexList.ReadFrom(_context, reader);
					ret[i] = __refvalue(__makeref(val), T);
				}
			}

			else if (typeOfT == typeof(WeightedIndexList)) {
				for (uint i = 0; i < len; i++) {
					var val = WeightedIndexList.ReadFrom(_context, reader);
					ret[i] = __refvalue(__makeref(val), T);
				}
			} 
            
            else if (typeOfT == typeof(Vector<float>)) {
                for (uint i = 0; i < len; i++) {
                    var val = new Vector<float>(_context, reader);
                    ret[i] = __refvalue(__makeref(val), T);
                }
            } 
            
            else if (typeOfT == typeof(Matrix<float>)) {
                for (uint i = 0; i < len; i++) {
                    var val = new Matrix<float>(_context, reader);
                    ret[i] = __refvalue(__makeref(val), T);
                }
            } 
            
            else if (typeOfT == typeof(Tensor3D<float>)) {
                for (uint i = 0; i < len; i++) {
                    var val = new Tensor3D<float>(_context, reader);
                    ret[i] = __refvalue(__makeref(val), T);
                }
            } 
            
            else if (typeOfT == typeof(Tensor4D<float>)) {
                for (uint i = 0; i < len; i++) {
                    var val = new Tensor4D<float>(_context, reader);
                    ret[i] = __refvalue(__makeref(val), T);
                }
            } 
            
            else if (typeOfT == typeof(BinaryData)) {
                for (uint i = 0; i < len; i++) {
                    var val = new BinaryData(reader);
                    ret[i] = __refvalue(__makeref(val), T);
                }
            }

			else 
				throw new NotImplementedException();

			return ret;
		}

        public static void Write<T>(BinaryWriter writer, T val)
        {
            var typeOfT = typeof(T);
            var valRef = __makeref(val);

            if (typeOfT == typeof(string)) {
                writer.Write(__refvalue(valRef, string));
            }

            else if (typeOfT == typeof(decimal)) {
                writer.Write(__refvalue(valRef, decimal));
            }

            else if (typeOfT == typeof(double)) {
                writer.Write(__refvalue(valRef, double));
            }

            else if (typeOfT == typeof(float)) {
                writer.Write(__refvalue(valRef, float));
            }

            else if (typeOfT == typeof(long)) {
                writer.Write(__refvalue(valRef, long));
            }

            else if (typeOfT == typeof(int)) {
                writer.Write(__refvalue(valRef, int));
            }

            else if (typeOfT == typeof(short)) {
                writer.Write(__refvalue(valRef, short));
            }

            else if (typeOfT == typeof(sbyte)) {
                writer.Write(__refvalue(valRef, sbyte));
            }

            else if (typeOfT == typeof(bool)) {
                writer.Write(__refvalue(valRef, bool));
            }

            else if (typeOfT == typeof(DateTime)) {
                writer.Write(__refvalue(valRef, DateTime).Ticks);
            }

            else if (typeOfT == typeof(IndexList)) {
                __refvalue(valRef, IndexList).WriteTo(writer);
            }

            else if (typeOfT == typeof(WeightedIndexList)) {
                __refvalue(valRef, WeightedIndexList).WriteTo(writer);
            }

            else if (typeOfT == typeof(Vector<float>)) {
                __refvalue(valRef, Vector<float>).WriteTo(writer);
            }

            else if (typeOfT == typeof(Matrix<float>)) {
                __refvalue(valRef, Matrix<float>).WriteTo(writer);
            }

            else if (typeOfT == typeof(Tensor3D<float>)) {
                __refvalue(valRef, Tensor3D<float>).WriteTo(writer);
            }

            else if (typeOfT == typeof(Tensor4D<float>)) {
                __refvalue(valRef, Tensor4D<float>).WriteTo(writer);
            }

            else if (typeOfT == typeof(BinaryData)) {
                __refvalue(valRef, BinaryData).WriteTo(writer);
            }

            else 
                throw new NotImplementedException();
        }

        public static void Write<T>(BinaryWriter writer, T[] values)
		{
			var typeOfT = typeof(T);
			var len = (uint)values.Length;
			writer.Write(len);

			if (typeOfT == typeof(string)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), string));
			}

            else if (typeOfT == typeof(decimal)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), decimal));
			}

			else if (typeOfT == typeof(double)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), double));
			}

			else if (typeOfT == typeof(float)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), float));
			}

            else if (typeOfT == typeof(long)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), long));
			}

			else if (typeOfT == typeof(int)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), int));
			}

            else if (typeOfT == typeof(short)) {
                for (uint i = 0; i < len; i++)
                    writer.Write(__refvalue(__makeref(values[i]), short));
            }

			else if (typeOfT == typeof(sbyte)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), sbyte));
			}

			else if (typeOfT == typeof(bool)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), bool));
			}

			else if (typeOfT == typeof(DateTime)) {
				for (uint i = 0; i < len; i++)
					writer.Write(__refvalue(__makeref(values[i]), DateTime).Ticks);
			}

			else if (typeOfT == typeof(IndexList)) {
				for (uint i = 0; i < len; i++)
					__refvalue(__makeref(values[i]), IndexList).WriteTo(writer);
			}

			else if (typeOfT == typeof(WeightedIndexList)) {
				for (uint i = 0; i < len; i++)
					__refvalue(__makeref(values[i]), WeightedIndexList).WriteTo(writer);
			}

			else if (typeOfT == typeof(Vector<float>)) {
				for (uint i = 0; i < len; i++)
					__refvalue(__makeref(values[i]), Vector<float>).WriteTo(writer);
			}

			else if (typeOfT == typeof(Matrix<float>)) {
				for (uint i = 0; i < len; i++)
					__refvalue(__makeref(values[i]), Matrix<float>).WriteTo(writer);
			}

			else if (typeOfT == typeof(Tensor3D<float>)) {
				for (uint i = 0; i < len; i++)
					__refvalue(__makeref(values[i]), Tensor3D<float>).WriteTo(writer);
			}

            else if (typeOfT == typeof(Tensor4D<float>)) {
                for (uint i = 0; i < len; i++)
                    __refvalue(__makeref(values[i]), Tensor4D<float>).WriteTo(writer);
            }

            else if (typeOfT == typeof(BinaryData)) {
                for (uint i = 0; i < len; i++)
                    __refvalue(__makeref(values[i]), BinaryData).WriteTo(writer);
            }

			else 
				throw new NotImplementedException();
		}

        //public static void Write(BinaryWriter writer, uint index, ColumnSize size)
        //{
        //    if (size == ColumnSize.Small)
        //        writer.Write((byte)index);
        //    else if (size == ColumnSize.Medium)
        //        writer.Write((ushort)index);
        //    else
        //        writer.Write(index);
        //}

        //public uint Read(BinaryReader reader, ColumnSize size)
        //{
        //    if (size == ColumnSize.Small)
        //        return reader.ReadByte();
        //    if (size == ColumnSize.Medium)
        //        return reader.ReadUInt16();
        //    return reader.ReadUInt32();
        //}
    }
}
