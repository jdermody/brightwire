using ProtoBuf;

namespace BrightWire.Models
{
    /// <summary>
    /// A serialised graph
    /// </summary>
    [ProtoContract]
    public class GraphModel
    {
        /// <summary>
        /// Data contract version number
        /// </summary>
        [ProtoMember(1)]
        public string Version { get; set; } = "2.0";

        /// <summary>
        /// The name of the graph
        /// </summary>
        [ProtoMember(2)]
        public string Name { get; set; }

        /// <summary>
        /// The primary execution graph
        /// </summary>
        [ProtoMember(3)]
        public ExecutionGraph Graph { get; set; }

        /// <summary>
        /// Optional data source associated with the model
        /// </summary>
        [ProtoMember(4)]
        public DataSourceModel DataSource { get; set; }
    }
}
