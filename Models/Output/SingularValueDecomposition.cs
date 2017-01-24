using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A matrix that has been decomposed as in https://en.wikipedia.org/wiki/Singular_value_decomposition
    /// </summary>
    public class SingularValueDecomposition : IDisposable
    {
        /// <summary>
        /// The U matrix
        /// </summary>
        public IMatrix U { get; private set; }

        /// <summary>
        /// The vector of singular values
        /// </summary>
        public IVector S { get; private set; }

        /// <summary>
        /// The transposed V matrix
        /// </summary>
        public IMatrix VT { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="u"></param>
        /// <param name="s"></param>
        /// <param name="vt"></param>
        public SingularValueDecomposition(IMatrix u, IVector s, IMatrix vt)
        {
            U = u;
            S = s;
            VT = vt;
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            U.Dispose();
            S.Dispose();
            VT.Dispose();
        }
    }
}
