namespace Xiangqi.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User> AddAsync(string username, string password = null, string displayName = null);
    Task<User> GetAsync(Guid id);
    Task<User> FindByNameAsync(string username);
    Task<bool> ChangeUsernameAsync(Guid userId, string newUsername);
    Task<bool> SetPasswordAsync(Guid userId, string newPassword);
}

public sealed class InMemoryUserRepo : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _byName = new(StringComparer.OrdinalIgnoreCase);

    public Task<User> AddAsync(string username, string displayName = null, string password = null)
    {
        if (_byName.ContainsKey(username))
            throw new InvalidOperationException("USERNAME already taken");

        var u = new User
        {
            Username = username,
            DisplayName = displayName ?? username,
            PasswordHash = password is null ? null : PasswordHash.Hash(password)
        };

        _byId[u.Id] = u;
        _byName[username] = u.Id;
        return Task.FromResult(u);
    }

    public Task<User> GetAsync(Guid id) => Task.FromResult(_byId.TryGetValue(id, out var u) ? u : null);

    public Task<User> FindByNameAsync(string username)
    {
        if (_byName.TryGetValue(username, out var id) && _byId.TryGetValue(id, out var u))
            return Task.FromResult<User>(u);
        return Task.FromResult<User>(null);
    }

    public Task<bool> ChangeUsernameAsync(Guid userId, string newUsername)
    {
        if (_byName.ContainsKey(newUsername))
            return Task.FromResult(false);

        if (!_byId.TryGetValue(userId, out var u))
            return Task.FromResult(false);


        _byName.TryRemove(u.Username, out _);
        u.Username = newUsername;
        _byName[newUsername] = u.Id;
        return Task.FromResult(true);

    }

    public Task<bool> SetPasswordAsync(Guid userId, string newPassword)
    {
        if (!_byId.TryGetValue(userId, out var u))
            return Task.FromResult(false);

        u.PasswordHash = PasswordHash.Hash(newPassword);
        return Task.FromResult(true);


    }



}


