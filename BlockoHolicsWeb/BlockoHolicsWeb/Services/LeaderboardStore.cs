using BlockoHolicsWeb.Models;

namespace BlockoHolicsWeb.Services
{
    public static class LeaderboardStore
    {
        private static readonly List<PlayerModel> _players = new List<PlayerModel>();
        private static readonly object _lock = new object();

        public static IReadOnlyList<PlayerModel> GetAll()
        {
            lock (_lock)
            {
                // return a copy ordered by rank
                return _players.OrderBy(p => p.Rank).ToList();
            }
        }

        public static void Add(PlayerModel p)
        {
            lock (_lock)
            {
                // Add then recompute ranks by ElapsedMs ascending
                _players.Add(p);
                var ordered = _players.OrderBy(x => x.ElapsedMs).ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    ordered[i].Rank = i + 1;
                }

                // replace internal list with ordered list
                _players.Clear();
                _players.AddRange(ordered);
            }
        }
    }
}
