using BlockoHolicsWeb.Data.Models;

namespace BlockoHolicsWeb.Contracts;

public interface IDbService
{
    public Task<IList<Player>> GetPlayers();

    public Task WritePlayer(Player player);
}