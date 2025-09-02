namespace Xiangqi.Infrastructure.Models;

public sealed class Room
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int Number { get; internal set; }
    public string Code => $"room{Number}";

    public Guid OwnerId { get; set; }
    public string Name { get; set; } = "Room";
    public RoomSettings Settings { get; set; } = new();

    public Guid? RedPlayerId { get; set; }
    public Guid? BlackPlayerId { get; set; }
    public HashSet<Guid> Spectators { get; } = new();
    public HashSet<Guid> Banned { get; } = new();

    public HashSet<Guid> Waiting { get; } = new();
    public bool Playing { get; set; } = false;
    public Guid? GameId { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}