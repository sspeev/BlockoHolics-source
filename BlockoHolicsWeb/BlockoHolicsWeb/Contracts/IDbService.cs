using BlockoHolicsWeb.Data.Models;

namespace BlockoHolicsWeb.Contracts;

public interface IDbService
{
    public Task<IList<Player>> GetPlayers();

    public Task WritePlayer(Player player);

    Task<bool> IsRecentRunExists(double elapsedSeconds, double windowSeconds = 2, string playerName = "Anonymous");
}