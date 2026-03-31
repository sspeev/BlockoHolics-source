using BlockoHolicsWeb.Data.Models;

namespace BlockoHolicsWeb.Services
{
    public interface IDbService
    {
        public Task<IEnumerable<Player>> GetPlayers();

        public Task WritePlayer(Player player);
    }
}
