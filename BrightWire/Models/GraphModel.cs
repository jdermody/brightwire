namespace BrightWire.Models
{
    /// <summary>
    /// A serialised graph
    /// </summary>
    public class GraphModel
    {
        /// <summary>
        /// Data contract version number
        /// </summary>
        public string Version { get; set; } = "2.0";

        /// <summary>
        /// The name of the graph
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The primary execution graph
        /// </summary>
        public ExecutionGraph Graph { get; set; }

        /// <summary>
        /// Optional data source associated with the model
        /// </summary>
        public DataSourceModel DataSource { get; set; }
    }
}
