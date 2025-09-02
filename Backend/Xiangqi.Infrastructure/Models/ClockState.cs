namespace Xiangqi.Infrastructure.Models;

public sealed class ClockState
{
    public int RedMs { get; set; }
    public int BlackMs { get; set; }
    public DateTime TurnStartUtc { get; set; }
    public string SideToMove { get; set; } = "red";
}
