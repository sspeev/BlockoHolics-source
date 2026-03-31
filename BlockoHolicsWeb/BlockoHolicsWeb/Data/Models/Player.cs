using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static BlockoHolicsWeb.Constants.DataConstants.PlayerConstants;

namespace BlockoHolicsWeb.Data.Models
{
    [Keyless]
    [Comment("Represents a player in the game.")]
    public class Player
    {
        [MaxLength(MaxPlayerName)]
        public string? Name { get; set; }

        [Required]
        public DateTime Time { get; set; }
    }
}
