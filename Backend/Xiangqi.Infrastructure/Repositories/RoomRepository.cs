namespace Xiangqi.Infrastructure.Repositories;

public interface IRoomRepository
{
    Task<Room> AddAsync(Room room);
    Task<Room> GetAsync(Guid id);
    Task<Room> GetByCodeAsync(string code);
    Task<IEnumerable<Room>> ListPublicAsync();
    Task<IEnumerable<Room>> ListAsync(Visibility? filter);
    Task RemoveAsync(Guid id);
}

public sealed class InMemoryRoomRepo : IRoomRepository
{
    private readonly ConcurrentDictionary<Guid, Room> _rooms = new();
    private readonly ConcurrentDictionary<string, Guid> _byCode = new(StringComparer.OrdinalIgnoreCase);
    private int _counter = 0;

    public Task<Room> AddAsync(Room room)
    {
        room.Number = Interlocked.Increment(ref _counter);
        _rooms[room.Id] = room;
        _byCode[room.Code] = room.Id;
        return Task.FromResult(room);
    }

    public Task<Room> GetAsync(Guid id) => Task.FromResult(_rooms.TryGetValue(id, out var r) ? r : null);

    public Task<Room> GetByCodeAsync(string code)
    {
        if (_byCode.TryGetValue(code, out var id) && _rooms.TryGetValue(id, out var room))
            return Task.FromResult<Room>(room);
        return Task.FromResult<Room>(null);
    }

    public Task<IEnumerable<Room>> ListPublicAsync() =>
        Task.FromResult<IEnumerable<Room>>(_rooms.Values.Where(r => r.Settings.Visibility == Visibility.Public));

    public Task<IEnumerable<Room>> ListAsync(Visibility? filter)
    {
        IEnumerable<Room> q = _rooms.Values;
        if (filter is not null)
            q = q.Where(r => r.Settings.Visibility == filter.Value);
        return Task.FromResult(q);
    }
    public Task RemoveAsync(Guid id)
    {
        if (_rooms.TryRemove(id, out var r)) _byCode.TryRemove(r.Code, out _);
        return Task.CompletedTask;
    }

}

