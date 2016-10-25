using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class SequenceSplit<T>
    {
        public IReadOnlyList<T> Training { get; set; }
        public IReadOnlyList<T> Test { get; set; }
    }
}
