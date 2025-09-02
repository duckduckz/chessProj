namespace Xiangqi.Infrastructure.Services;

public interface IRoomService
{
    Task<Room> CreateAsync(Guid ownerId, string name, RoomSettings settings);
    Task<bool> JoinAsync(Guid roomId, Guid userId, string role, string password);
    Task LeaveAsync(Guid roomId, Guid userId);

    Task<bool> JoinWaitingAsync(Guid roomId, Guid userId, string password);
    Task<bool> AssignSeatAsync(Guid roomId, Guid ownerId, Guid userId, string role);
    Task<bool> UnassignSeatAsync(Guid roomId, Guid ownerId, string role);
}

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepo;
    public RoomService(IRoomRepository roomRepo) => _roomRepo = roomRepo;

    public async Task<Room> CreateAsync(Guid ownerId, string name, RoomSettings settings)
    {
        var room = new Room { OwnerId = ownerId, Name = name, Settings = settings };
        await _roomRepo.AddAsync(room);
        return room;
    }

    public async Task<bool> JoinAsync(Guid roomId, Guid userId, string role, string password)
    {
        var r = await _roomRepo.GetAsync(roomId) ??
            throw new InvalidOperationException("Room not found");

        if (r.Banned.Contains(userId))
            return false;

        if (r.Settings.Visibility == Visibility.Private && r.Settings.Password != password)
            return false;

        if (r.RedPlayerId is not null && r.BlackPlayerId is not null)
            return false;

        switch ((role ?? "spectator").ToLowerInvariant())
        {
            case "red":
                if (r.RedPlayerId is not null) return false;
                r.RedPlayerId = userId;
                break;
            case "black":
                if (r.BlackPlayerId is not null) return false;
                r.BlackPlayerId = userId;
                break;
            default:
                if (r.Spectators.Count >= r.Settings.SpectatorLimit) return false;
                r.Spectators.Add(userId);
                break;
        }
        return true;
    }

    public async Task LeaveAsync(Guid roomId, Guid userId)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (r.RedPlayerId == userId) r.RedPlayerId = null;
        else if (r.BlackPlayerId == userId) r.BlackPlayerId = null;
        else r.Spectators.Remove(userId);
    }

    public async Task<bool> JoinWaitingAsync(Guid roomId, Guid userId, string password)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        if (r.Banned.Contains(userId)) return false;
        if (r.Settings.Visibility == Visibility.Private && r.Settings.Password != password) return false;

        if (r.RedPlayerId is not null && r.BlackPlayerId is not null)
            return false;
            
        r.Waiting.Add(userId);
        // add as spectator so they can watch while waiting
        r.Spectators.Add(userId);
        return true;
    }

    public async Task<bool> AssignSeatAsync(Guid roomId, Guid ownerId, Guid userId, string role)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (r.OwnerId != ownerId) return false;              // only owner can seat
        if (!r.Waiting.Contains(userId) && !r.Spectators.Contains(userId)) return false;

        switch (role.ToLowerInvariant())
        {
            case "red":
                if (r.RedPlayerId is not null) return false;
                r.RedPlayerId = userId;
                break;
            case "black":
                if (r.BlackPlayerId is not null) return false;
                r.BlackPlayerId = userId;
                break;
            default:
                return false;
        }

        r.Waiting.Remove(userId);
        // keep in Spectators
        // r.Spectators.Remove(userId);
        return true;
    }

    public async Task<bool> UnassignSeatAsync(Guid roomId, Guid ownerId, string role)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (r.OwnerId != ownerId) return false;

        switch (role.ToLowerInvariant())
        {
            case "red":
                if (r.RedPlayerId is null) return false;
                r.Spectators.Add(r.RedPlayerId.Value);
                r.Waiting.Add(r.RedPlayerId.Value);
                r.RedPlayerId = null;
                return true;

            case "black":
                if (r.BlackPlayerId is null) return false;
                r.Spectators.Add(r.BlackPlayerId.Value);
                r.Waiting.Add(r.BlackPlayerId.Value);
                r.BlackPlayerId = null;
                return true;

            default:
                return false;
        }
    }

}
