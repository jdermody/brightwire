using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class SingularValueDecomposition : IDisposable
    {
        public IMatrix U { get; private set; }
        public IVector S { get; private set; }
        public IMatrix VT { get; private set; }

        public SingularValueDecomposition(IMatrix u, IVector s, IMatrix vt)
        {
            U = u;
            S = s;
            VT = vt;
        }

        public void Dispose()
        {
            U.Dispose();
            S.Dispose();
            VT.Dispose();
        }
    }
}
