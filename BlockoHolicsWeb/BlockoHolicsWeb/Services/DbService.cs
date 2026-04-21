using BlockoHolicsWeb.Contracts;
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
            .OrderByDescending(p => p.IsFinished)
            .ThenBy(p => p.ElapsedSeconds)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task WritePlayer(Player player)
    {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsRecentRunExists(double elapsedSeconds, double windowSeconds = 2, string playerName = "Anonymous")
    {
        return await _context.Players
            .Where(p => Math.Abs(p.ElapsedSeconds - elapsedSeconds) <= windowSeconds)
            .Where(p => p.Name == playerName)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync() != null;
    }
}