using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class SplitTableRequest
    {
        [Required] public double TrainingPercentage { get; set; }
    }
}
