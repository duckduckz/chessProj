namespace Xiangqi.Infrastructure.Models;

public sealed class MoveRecord
{
    public int Ply { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public string FenAfter { get; set; } = default!;
    public Guid ByUserId { get; set; }
    public int TimeSpentMs { get; set; }
}