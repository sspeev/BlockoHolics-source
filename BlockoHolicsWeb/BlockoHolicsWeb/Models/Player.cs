namespace Blockoholics.Models
{
    public class Player
    {
        public int Rank { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        // elapsed time in milliseconds for sorting and precision
        public long ElapsedMs { get; set; }
    }
}