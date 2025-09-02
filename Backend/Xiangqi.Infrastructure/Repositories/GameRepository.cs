namespace Xiangqi.Infrastructure.Repositories;

public interface IGameRepository
{
    Task<Game> AddAsync(Game g);
    Task<Game> GetAsync(Guid id);
    Task SaveAsync(Game g);
}

public sealed class InMemoryGameRepo : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();
    public Task<Game> AddAsync(Game g) { _games[g.Id] = g; return Task.FromResult(g); }
    public Task<Game> GetAsync(Guid id) => Task.FromResult(_games.TryGetValue(id, out var g) ? g : null);
    public Task SaveAsync(Game g) { _games[g.Id] = g; return Task.CompletedTask; }
}
