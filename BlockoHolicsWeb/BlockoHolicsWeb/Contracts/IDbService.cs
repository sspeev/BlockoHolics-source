using BlockoHolicsWeb.Data.Models;

namespace BlockoHolicsWeb.Contracts;

public interface IDbService
{
    public Task<IList<Player>> GetPlayers();

    public Task WritePlayer(Player player);

    Task<bool> IsRecentRunExists(int elapsedSeconds, int windowSeconds = 2, string playerName = "Anonymous");
}