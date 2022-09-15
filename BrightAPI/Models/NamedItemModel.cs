using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models
{
    public class NamedItemModel
    {
        [Required] public string Id { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
    }
}
