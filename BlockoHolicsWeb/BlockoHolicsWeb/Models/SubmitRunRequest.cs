using System.ComponentModel.DataAnnotations;

namespace BlockoHolicsWeb.Models;

public class SubmitRunRequest
{
    [Required(ErrorMessage = "Player name is required")]
    [StringLength(100, MinimumLength = 1)]
    public string PlayerName { get; set; } = "Anonymous";

    [Range(1, long.MaxValue, ErrorMessage = "Elapsed time must be positive")]
    public long ElapsedMs { get; set; }

    public bool IsFinished { get; set; }
}