using BlockoHolicsWeb.Data;
using BlockoHolicsWeb.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlockoHolicsWeb.Services;

public class DbService(BlockoHolicsDbContext context) : IDbService
{
    private readonly BlockoHolicsDbContext _context = context;
    public async Task<IList<Player>> GetPlayers()
    {
        return await _context.Players
            .AsNoTracking()
            .Where(p => p.ElapsedSeconds > 0)
            .Select(p => new Player
            {
                Name = p.Name,
                ElapsedSeconds = p.ElapsedSeconds,
                IsFinished = p.IsFinished
            })
            .OrderBy(p => p.ElapsedSeconds)
            .ToListAsync();
    }

    public async Task WritePlayer(Player player)
    {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();
    }
}

