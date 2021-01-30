using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Iterators
{
    public class StringIterator : SimpleIterator<char>
    {
        readonly string _data;

        public StringIterator(string str) : base(str.ToCharArray())
        {
            _data = str;
        }

        public override bool Equals(object obj)
        {
            var other = (StringIterator)obj;
            return other != null && (Length == other.Length && _pos == other._pos && _data.Equals(other._data));
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode() ^ _pos;
        }

        public override string ToString()
        {
            return _data.Substring(_pos);
        }

        public new StringIterator Clone()
        {
            var ret = new StringIterator(_data)
            {
                _pos = _pos
            };
            foreach (var item in _contextStack)
                ret._contextStack.Push(item);
            return ret;
        }

        public string Data => _data;

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

        public bool IsMatch(char[] list)
        {
            var curr = Peek();
            foreach (var item in list)
            {
                if (curr == item)
                    return true;
            }
            return false;
        }

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

        public new StringIterator GetRange(int start, int end)
        {
            var length = end - start;
            if (length > _list.Count - start)
                length = _list.Count - start;
            if (start < _list.Count && length > 0)
                return new StringIterator(new string(_list.Skip(start).Take(length).ToArray()));
            return new StringIterator("");
        }
    }
}
