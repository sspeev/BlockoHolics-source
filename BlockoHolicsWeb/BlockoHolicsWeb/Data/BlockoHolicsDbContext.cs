using BlockoHolicsWeb.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlockoHolicsWeb.Data;

public class BlockoHolicsDbContext(DbContextOptions<BlockoHolicsDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
}
