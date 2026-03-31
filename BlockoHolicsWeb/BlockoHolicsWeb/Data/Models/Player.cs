using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static BlockoHolicsWeb.Constants.DataConstants.PlayerConstants;

namespace BlockoHolicsWeb.Data.Models
{
    [Comment("Represents a player in the game.")]
    public class Player
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(MaxPlayerName)]
        public string? Name { get; set; } = "Anonymous";

        [Required]
        public int ElapsedSeconds { get; set; }
    }
}
