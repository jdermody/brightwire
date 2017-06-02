using ProtoBuf;

namespace BrightWire.Models
{
    /// <summary>
    /// Serialises an adaptive data source - that is, a data source that takes the output from a preliminary output graph and sends it to the primary graph
    /// </summary>
    [ProtoContract]
    public class DataSourceModel
    {
        /// <summary>
        /// Data contract version number
        /// </summary>
        [ProtoMember(1)]
        public string Version { get; set; } = "2.0";

        /// <summary>
        /// The name of the data source
        /// </summary>
        [ProtoMember(2)]
        public string Name { get; set; }

        /// <summary>
        /// The size of each input training data item
        /// </summary>
        [ProtoMember(3)]
        public int InputSize { get; set; }

        /// <summary>
        /// The size of each training item output (classification label)
        /// </summary>
        [ProtoMember(4)]
        public int OutputSize { get; set; }

        /// <summary>
        /// The preliminary graph
        /// </summary>
        [ProtoMember(5)]
        public ExecutionGraph Graph { get; set; }
    }
}
