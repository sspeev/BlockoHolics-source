namespace BlockoHolicsWeb.Models;

public class PlayerModel
{
    public int Rank { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Time { get; set; } = string.Empty;

    public long ElapsedMs { get; set; }

    public bool IsFinished { get; set; }
}
