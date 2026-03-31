using Blockoholics.Models;
using System.Collections.Generic;
using System.Linq;

namespace BlockoHolicsWeb.Services
{
    public static class LeaderboardStore
    {
        private static readonly List<Player> _players = new List<Player>();
        private static readonly object _lock = new object();

        public static IReadOnlyList<Player> GetAll()
        {
            lock (_lock)
            {
                // return a copy ordered by rank
                return _players.OrderBy(p => p.Rank).ToList();
            }
        }

        public static void Add(Player p)
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
