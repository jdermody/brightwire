using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit
{
    /// <summary>
    /// CUDA dim3. In difference to the CUDA dim3 type, this dim3 initializes to 0 for each element.
    /// dim3 should be value-types so that we can pack it in an array. But C# value types (structs)
    /// do not garantee to execute an default constructor, why it doesn't exist.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct dim3
    {
        /// <summary>
        /// X
        /// </summary>
        public uint x;
        /// <summary>
        /// Y
        /// </summary>
        public uint y;
        /// <summary>
        /// Z
        /// </summary>
        public uint z;

        #region Operator Methods
        /// <summary>
        /// per element Add
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Add(dim3 src, dim3 value)
        {
            dim3 ret = new dim3(src.x + value.x, src.y + value.y, src.z + value.z);
            return ret;
        }

        /// <summary>
        /// per element Add
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Add(dim3 src, uint value)
        {
            dim3 ret = new dim3(src.x + value, src.y + value, src.z + value);
            return ret;
        }

        /// <summary>
        /// per element Add
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Add(uint src, dim3 value)
        {
            dim3 ret = new dim3(src + value.x, src + value.y, src + value.z);
            return ret;
        }

        /// <summary>
        /// per element Substract
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Subtract(dim3 src, dim3 value)
        {
            dim3 ret = new dim3(src.x - value.x, src.y - value.y, src.z - value.z);
            return ret;
        }

        /// <summary>
        /// per element Substract
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Subtract(dim3 src, uint value)
        {
            dim3 ret = new dim3(src.x - value, src.y - value, src.z - value);
            return ret;
        }

        /// <summary>
        /// per element Substract
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Subtract(uint src, dim3 value)
        {
            dim3 ret = new dim3(src - value.x, src - value.y, src - value.z);
            return ret;
        }

        /// <summary>
        /// per element Multiply
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Multiply(dim3 src, dim3 value)
        {
            dim3 ret = new dim3(src.x * value.x, src.y * value.y, src.z * value.z);
            return ret;
        }

        /// <summary>
        /// per element Multiply
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Multiply(dim3 src, uint value)
        {
            dim3 ret = new dim3(src.x * value, src.y * value, src.z * value);
            return ret;
        }

        /// <summary>
        /// per element Multiply
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Multiply(uint src, dim3 value)
        {
            dim3 ret = new dim3(src * value.x, src * value.y, src * value.z);
            return ret;
        }

        /// <summary>
        /// per element Divide
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Divide(dim3 src, dim3 value)
        {
            dim3 ret = new dim3(src.x / value.x, src.y / value.y, src.z / value.z);
            return ret;
        }

        /// <summary>
        /// per element Divide
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Divide(dim3 src, uint value)
        {
            dim3 ret = new dim3(src.x / value, src.y / value, src.z / value);
            return ret;
        }

        /// <summary>
        /// per element Divide
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 Divide(uint src, dim3 value)
        {
            dim3 ret = new dim3(src / value.x, src / value.y, src / value.z);
            return ret;
        }
        #endregion

        #region operators
        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator +(dim3 src, dim3 value)
        {
            return Add(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator +(dim3 src, uint value)
        {
            return Add(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator +(uint src, dim3 value)
        {
            return Add(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator -(dim3 src, dim3 value)
        {
            return Subtract(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator -(dim3 src, uint value)
        {
            return Subtract(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator -(uint src, dim3 value)
        {
            return Subtract(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator *(dim3 src, dim3 value)
        {
            return Multiply(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator *(dim3 src, uint value)
        {
            return Multiply(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator *(uint src, dim3 value)
        {
            return Multiply(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator /(dim3 src, dim3 value)
        {
            return Divide(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator /(dim3 src, uint value)
        {
            return Divide(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dim3 operator /(uint src, dim3 value)
        {
            return Divide(src, value);
        }

        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator ==(dim3 src, dim3 value)
        {
            return src.Equals(value);
        }
        /// <summary>
        /// per element
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator !=(dim3 src, dim3 value)
        {
            return !(src == value);
        }
        /// <summary>
        /// implicit cast
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator dim3(int value)
        {
            return new dim3(value);
        }
        /// <summary>
        /// implicit cast
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator dim3(uint value)
        {
            return new dim3(value);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is dim3)) return false;

            dim3 value = (dim3)obj;

            bool ret = true;
            ret &= this.x == value.x;
            ret &= this.y == value.y;
            ret &= this.z == value.z;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Equals(dim3 value)
        {
            bool ret = true;
            ret &= this.x == value.x;
            ret &= this.y == value.y;
            ret &= this.z == value.z;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}; {1}; {2})", this.x, this.y, this.z);
        }
        #endregion

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xValue">X</param>
        /// <param name="yValue">Y</param>
        /// <param name="zValue">Z</param>
        public dim3(uint xValue, uint yValue, uint zValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = zValue;
        }
        /// <summary>
        /// .z = 1
        /// </summary>
        /// <param name="xValue">X</param>
        /// <param name="yValue">Y</param>
        public dim3(uint xValue, uint yValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = 1;
        }
        /// <summary>
        /// In contrast to other vector types the .y and .z values are set to 1 and not to val!
        /// </summary>
        /// <param name="val"></param>
        public dim3(uint val)
        {
            this.x = val;
            this.y = 1;
            this.z = 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xValue">X</param>
        /// <param name="yValue">Y</param>
        /// <param name="zValue">Z</param>
        public dim3(int xValue, int yValue, int zValue)
        {
            this.x = (uint)xValue;
            this.y = (uint)yValue;
            this.z = (uint)zValue;
        }
        /// <summary>
        /// .z = 1
        /// </summary>
        /// <param name="xValue">X</param>
        /// <param name="yValue">Y</param>
        public dim3(int xValue, int yValue)
        {
            this.x = (uint)xValue;
            this.y = (uint)yValue;
            this.z = 1;
        }
        /// <summary>
        /// In contrast to other vector types the .y and .z values are set to 1 and not to val!
        /// </summary>
        /// <param name="val"></param>
        public dim3(int val)
        {
            this.x = (uint)val;
            this.y = 1;
            this.z = 1;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Component wise minimum as the CUDA function imini
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public static dim3 Min(dim3 aValue, dim3 bValue)
        {
            return new dim3(System.Math.Min(aValue.x, bValue.x), System.Math.Min(aValue.y, bValue.y), System.Math.Min(aValue.z, bValue.z));
        }

        /// <summary>
        /// Component wise maximum as the CUDA function imaxi
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public static dim3 Max(dim3 aValue, dim3 bValue)
        {
            return new dim3(System.Math.Max(aValue.x, bValue.x), System.Math.Max(aValue.y, bValue.y), System.Math.Max(aValue.z, bValue.z));
        }
        #endregion

        #region SizeOf
        /// <summary>
        /// Gives the size of this type in bytes. <para/>
        /// Is equal to <c>Marshal.SizeOf(dim3);</c>
        /// </summary>
        public static uint SizeOf
        {
            get
            {
                return (uint)Marshal.SizeOf(typeof(dim3));
            }
        }

        /// <summary>
        /// Gives the size of this type in bytes. <para/>
        /// Is equal to <c>Marshal.SizeOf(this);</c>
        /// </summary>
        public uint Size
        {
            get
            {
                return (uint)Marshal.SizeOf(this);
            }
        }
        #endregion
    }
}
