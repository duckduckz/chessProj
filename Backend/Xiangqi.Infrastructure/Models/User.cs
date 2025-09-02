namespace Xiangqi.Infrastructure.Models;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public HashSet<Guid> Blocked { get; } = new();

    public string Name { get; set; }
    public string ProfilePhotoBase64 { get; set; }
    
    public DateTime? LastActiveUtc { get; set; }

    // auth
    public string PasswordHash { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivateUtc { get; set; }
    public UserStates Stats { get; } = new();
}