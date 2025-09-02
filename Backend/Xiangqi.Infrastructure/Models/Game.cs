namespace Xiangqi.Infrastructure.Models;

public sealed class Game
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public string CurrentFen { get; set; } = default!;
    public List<MoveRecord> Moves { get; } = new();
    public GameResult Result { get; set; } = GameResult.Ongoing;
    public string ResultReason { get; set; }
    public ClockState Clock { get; set; } = new();
}