using System.Diagnostics;
using System.IO;

namespace BrightWire
{
    /// <summary>
    /// Standard serialisation interface
    /// </summary>
    public interface ICanSerialiseToStream
    {
        /// <summary>
        /// Writes the current object state to the stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        void SerialiseTo(Stream stream);

        /// <summary>
        /// Reads the current object state from the stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="clear">True to clear the existing state</param>
        void DeserialiseFrom(Stream stream, bool clear);
    }

    //public interface ICanTrace
    //{
    //    string Trace();
    //}

    // other declarations in nested files...
}
