using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Iterator
{
    /// <summary>
    /// Iterates characters in a string
    /// </summary>
    public class StringIterator : SimpleIterator<char>
    {
        readonly string _str;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="str">String to iterate</param>
        public StringIterator(string str) : base(str.ToCharArray(), '\0')
        {
            _str = str;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is StringIterator other && Length == other.Length && Position == other.Position && _data.Equals(other._data);
        }

        /// <inheritdoc />
        public override int GetHashCode() => _str.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => _str[Position..];

        /// <summary>
        /// Clones this iterator
        /// </summary>
        /// <returns></returns>
        public new StringIterator Clone()
        {
            var ret = new StringIterator(_str)
            {
                _pos = _pos
            };
            foreach (var item in _contextStack)
                ret._contextStack.Push(item);
            return ret;
        }

        /// <summary>
        /// Underlying string
        /// </summary>
        public string Data => _str;

        /// <summary>
        /// All lines in the string
        /// </summary>
        public IEnumerable<string> Lines
        {
            get
            {
                var curr = new StringBuilder();

                while (HasData)
                {
                    var ch = GetNext();
                    if (ch == '\n')
                    {
                        yield return curr.ToString();
                        curr.Clear();
                    }
                    else if (ch != '\r')
                        curr.Append(ch);
                }
            }
        }

        /// <summary>
        /// Checks if a sequence of characters matches at the current position
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool Matches(IEnumerable<char> list)
        {
            var curr = Peek();
            foreach (var item in list)
            {
                if (curr == item)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a string matches at the current position
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool Matches(string str)
        {
            int offset = 0;
            foreach (var ch in str)
            {
                if (Peek(offset++) != ch)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a new string iterator for a specific section of this iterator
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <returns></returns>
        public new StringIterator GetRange(int start, int end)
        {
            var length = end - start;
            if (length > _data.Length - start)
                length = _data.Length - start;
            if (start < _data.Length && length > 0)
                return new StringIterator(new string(_data.Skip(start).Take(length).ToArray()));
            return new StringIterator("");
        }
    }
}
