using System.ComponentModel.DataAnnotations;
using static BlockoHolicsWeb.Constants.DataConstants.PlayerConstants;

namespace BlockoHolicsWeb.Models;

public class PlayerModel
{
    public int Rank { get; set; }

    [MaxLength(MaxPlayerName)]
    public string Name { get; set; } = string.Empty;

    public string Time { get; set; } = string.Empty;

    public long ElapsedMs { get; set; }

    public bool IsFinished { get; set; }
}
