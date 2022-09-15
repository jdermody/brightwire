using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models
{
    public class NameValueModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Value { get; set; } = string.Empty;
    }
}
