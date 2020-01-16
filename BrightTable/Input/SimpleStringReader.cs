using System;
using System.Collections.Generic;
using System.Text;

namespace BrightTable.Input
{
    class SimpleStringReader : IStringIterator
    {
        readonly string _str;
        int _pos = 0;

        public SimpleStringReader(string str)
        {
            _str = str;
        }

        public long Position => _pos;
        public long ProgressPercent => _str.Length > 0 ? Position * 100 / _str.Length : 100;
        public char Next() => Position < _str.Length ? _str[_pos++] : '\0';
    }
}
