using ProtoBuf;

namespace BrightWire.Models
{
    /// <summary>
    /// An array of indexed strings
    /// </summary>
    [ProtoContract]
    public class StringTable
    {
        /// <summary>
        /// The array of indexed strings
        /// </summary>
        [ProtoMember(1)]
        public string[] Data { get; set; }
    }
}
