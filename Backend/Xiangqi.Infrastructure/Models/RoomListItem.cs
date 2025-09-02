namespace Xiangqi.Infrastructure.Models;

public sealed record RoomListItem(
    Guid Id,
    string Code,
    string Name,
    bool Playing,
    string Visibility,
    bool IsPrivate,
    Guid? RedPlayerId,
    Guid? BlackPlayerId,
    bool IsFull,
    int WaitingCount,
    int SpectatorCount,
    Guid OwnerId
);